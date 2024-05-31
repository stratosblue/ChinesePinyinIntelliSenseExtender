#nullable enable

using System.Buffers;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Threading;
using ChinesePinyinIntelliSenseExtender.Internal;
using ChinesePinyinIntelliSenseExtender.Options;
using ChinesePinyinIntelliSenseExtender.Util;

using Microsoft.VisualStudio.Core.Imaging;
using Microsoft.VisualStudio.Imaging;
using Microsoft.VisualStudio.Language.Intellisense.AsyncCompletion;
using Microsoft.VisualStudio.Language.Intellisense.AsyncCompletion.Data;
using Microsoft.VisualStudio.Text;

namespace ChinesePinyinIntelliSenseExtender.Intellisense.AsyncCompletion;

internal class IdeographAsyncCompletionSource : CompletionSourceBase, IAsyncCompletionSource
{
    #region Private 字段

    private static readonly CompletionFilter s_chineseFilter = new("表意文字代码", "C", new(KnownMonikers.Attribute.ToImageId(), "表意文字代码"));

    private static readonly ImmutableArray<CompletionFilter> s_chineseFilters = ImmutableArray.Create(s_chineseFilter);

    private readonly IEnumerable<IAsyncCompletionSource> _otherAsyncCompletionSources;

    #endregion Private 字段

    #region Public 构造函数

    public IdeographAsyncCompletionSource(IEnumerable<IAsyncCompletionSource> otherAsyncCompletionSources, GeneralOptions options)
        : base(options)
    {
        _otherAsyncCompletionSources = otherAsyncCompletionSources ?? throw new ArgumentNullException(nameof(otherAsyncCompletionSources));
    }

    #endregion Public 构造函数

    #region Public 方法

    public async Task<CompletionContext?> GetCompletionContextAsync(IAsyncCompletionSession session, CompletionTrigger trigger, SnapshotPoint triggerLocation, SnapshotSpan applicableToSpan, CancellationToken token)
    {
        if (s_completionContextRecursionTag.Value
            || !Options.Enable
            || Options.AsyncCompletionMode != AsyncCompletionMode.Default)
        {
            return null;
        }

        try
        {
            s_completionContextRecursionTag.Value = true;

            var getInputMethodDictionaryGroupTask = GetInputMethodDictionaryGroupAsync();

            var tasks = _otherAsyncCompletionSources.Select(m => m.GetCompletionContextAsync(session, trigger, triggerLocation, applicableToSpan, token)).ToArray();

            await Task.WhenAll(tasks);

            token.ThrowIfCancellationRequested();

            var shouldProcessChecker = StringPreMatchUtil.GetPreCheckPredicate(Options.PreMatchType, Options.PreCheckRule);

            var allCompletionItems = tasks.SelectMany(static m => m.Status == TaskStatus.RanToCompletion && m.Result?.Items is not null ? m.Result.Items.AsEnumerable() : Array.Empty<CompletionItem>());

            if (!allCompletionItems.Any())
            {
                return null;
            }

            var inputMethodDictionaryGroup = await getInputMethodDictionaryGroupTask;

            var count = tasks.Sum(static m => m.Status == TaskStatus.RanToCompletion && m.Result?.Items is not null ? m.Result.Items.Length : 0);

            var itemBuffer = ArrayPool<CompletionItem>.Shared.Rent(count * 5);  //预留足够多的空间，避免字典过多导致的问题
            try
            {
                int bufferIndex = 0;
                allCompletionItems.AsParallel()
                                  .WithCancellation(token)
                                  .ForAll(m => CreateCompletionItemWithConvertion(m, inputMethodDictionaryGroup, shouldProcessChecker, itemBuffer, ref bufferIndex));

                if (bufferIndex > 0)
                {
                    return new CompletionContext(ImmutableArray.Create(itemBuffer, 0, bufferIndex));
                }
                return null;
            }
            finally
            {
                ArrayPool<CompletionItem>.Shared.Return(itemBuffer);
            }
        }
        finally
        {
            s_completionContextRecursionTag.Value = false;
        }
    }

    public Task<object> GetDescriptionAsync(IAsyncCompletionSession session, CompletionItem item, CancellationToken token)
    {
        if (item.Properties.TryGetProperty<CompletionItem>(this, out var originCompletionItem)
            && originCompletionItem.Source is IAsyncCompletionSource asyncCompletionSource)
        {
            Debug.WriteLine($"GetDescriptionAsync {originCompletionItem} from {asyncCompletionSource}.");
            return asyncCompletionSource.GetDescriptionAsync(session, originCompletionItem, token);
        }

        Debug.WriteLine($"GetDescriptionAsync {item} by simple conact.");
        return Task.FromResult<object>($"{item.DisplayText} - {item.FilterText}");
    }

    public CompletionStartData InitializeCompletion(CompletionTrigger trigger, SnapshotPoint triggerLocation, CancellationToken token)
    {
        return CompletionStartData.ParticipatesInCompletionIfAny;
    }

    #endregion Public 方法

    #region impl

    private void CreateCompletionItemWithConvertion(CompletionItem originCompletionItem, InputMethodDictionaryGroup inputMethodDictionaryGroup, IPreCheckPredicate shouldProcessCheck, CompletionItem[] itemBuffer, ref int bufferIndex)
    {
        var originInsertText = originCompletionItem.InsertText;

        if (!shouldProcessCheck.Check(originInsertText))
        {
            return;
        }

        var spellings = inputMethodDictionaryGroup.FindAll(originInsertText);

        if (spellings.Length == 0)
        {
            return;
        }

        if (Options.EnableFSharpSupport
            && originCompletionItem.Properties.TryGetProperty("RoslynCompletionItemData", out object data)
            && TryGetRoslynItemNameInCode(data, out var nameInCode))
        {
            originInsertText = nameInCode!;
        }

        if (Options.SingleWordsDisplay)
        {
            foreach (var spelling in spellings)
            {
                itemBuffer[Interlocked.Increment(ref bufferIndex) - 1] = CreateCompletionItem(originCompletionItem, originInsertText, spelling);
            }
        }
        else if (Options.EnableMultipleSpellings)
        {
            itemBuffer[Interlocked.Increment(ref bufferIndex) - 1] = CreateCompletionItem(originCompletionItem, originInsertText, string.Join("/", spellings));
        }
        else
        {
            itemBuffer[Interlocked.Increment(ref bufferIndex) - 1] = CreateCompletionItem(originCompletionItem, originInsertText, spellings[0]);
        }
    }

    #region CreateCompletionItem

    private CompletionItem CreateCompletionItem(CompletionItem originCompletionItem, string originInsertText, string spelling)
    {
        var newCompletionItem = new CompletionItem(displayText: FormatString(Options.DisplayTextFormat, originCompletionItem.DisplayText, spelling),
                                                   source: this,
                                                   icon: originCompletionItem.Icon,
                                                   filters: s_chineseFilters,
                                                   suffix: FormatString(Options.DisplaySuffixFormat, originCompletionItem.Suffix, spelling),
                                                   insertText: originInsertText,
                                                   sortText: spelling,
                                                   filterText: spelling,
                                                   automationText: originCompletionItem.AutomationText,
                                                   attributeIcons: originCompletionItem.AttributeIcons,
                                                   commitCharacters: originCompletionItem.CommitCharacters,
                                                   applicableToSpan: originCompletionItem.ApplicableToSpan,
                                                   isCommittedAsSnippet: originCompletionItem.IsCommittedAsSnippet,
                                                   isPreselected: originCompletionItem.IsPreselected);

        newCompletionItem.Properties.AddProperty(this, originCompletionItem);

        return newCompletionItem;
    }

    #endregion CreateCompletionItem

    #region NameInCode

    private static bool TryGetRoslynItemNameInCode(object data, out string? nameInCode)
    {
        // 获取 F# 中特殊标识符（包含空格等特殊字符）的实际写法。F# 并没有将真正要写到代码里的内容存到 CompletionItem.InsertText 里，而是放在了
        // CompletionItem.Properties["RoslynCompletionItemData"].Value.Properties["NameInCode"] 中，因此需要用反射来获取。
        // 下面是 vs F# 扩展插入提示项的方法：
        // https://github.com/dotnet/fsharp/blob/main/vsintegration/src/FSharp.Editor/Completion/CompletionProvider.fs#L250

        //TODO 优化
        var dataType = data.GetType();
        if (string.Equals("Microsoft.CodeAnalysis.Editor.Implementation.IntelliSense.AsyncCompletion.CompletionItemData", dataType.FullName, StringComparison.Ordinal))
        {
            var roslynItem = dataType.GetProperty("RoslynItem").GetMethod.Invoke(data, Array.Empty<object>());
            var properties = (ImmutableDictionary<string, string>)roslynItem.GetType().GetProperty("Properties").GetMethod.Invoke(roslynItem, Array.Empty<object>());

            return properties.TryGetValue("NameInCode", out nameInCode);
        }
        nameInCode = null;
        return false;
    }

    #endregion NameInCode

    #endregion impl
}
