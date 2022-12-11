#nullable enable

using System.Buffers;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Threading;

using ChinesePinyinIntelliSenseExtender.Options;
using ChinesePinyinIntelliSenseExtender.Util;

using Microsoft.VisualStudio.Core.Imaging;
using Microsoft.VisualStudio.Imaging;
using Microsoft.VisualStudio.Language.Intellisense.AsyncCompletion;
using Microsoft.VisualStudio.Language.Intellisense.AsyncCompletion.Data;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Utilities;

namespace ChinesePinyinIntelliSenseExtender;

internal class PinyinAsyncCompletionSource : IAsyncCompletionSource
{
    #region Private 字段

    private static readonly CompletionFilter s_chineseFilter = new("中文代码", "C", new(KnownMonikers.Attribute.ToImageId(), "中文代码"));

    private static readonly ImmutableArray<CompletionFilter> s_chineseFilters = ImmutableArray.Create(s_chineseFilter);

    /// <summary>
    /// <see cref="GetCompletionContextAsync(IAsyncCompletionSession, CompletionTrigger, SnapshotPoint, SnapshotSpan, CancellationToken)"/> 递归标记
    /// </summary>
    private static readonly AsyncLocal<bool> s_getCompletionContextRecursionTag = new();

    private readonly GeneralOptions _options;
    private readonly IEnumerable<IAsyncCompletionSource> _otherAsyncCompletionSources;
    private InputMethodDictionaryGroup? _inputMethodDictionaryGroup;

    #endregion Private 字段

    #region Public 构造函数

    public PinyinAsyncCompletionSource(IEnumerable<IAsyncCompletionSource> otherAsyncCompletionSources, GeneralOptions options)
    {
        _otherAsyncCompletionSources = otherAsyncCompletionSources ?? throw new ArgumentNullException(nameof(otherAsyncCompletionSources));
        _options = options ?? throw new ArgumentNullException(nameof(options));
    }

    #endregion Public 构造函数

    #region Public 方法

    public async Task<CompletionContext?> GetCompletionContextAsync(IAsyncCompletionSession session, CompletionTrigger trigger, SnapshotPoint triggerLocation, SnapshotSpan applicableToSpan, CancellationToken token)
    {
        if (s_getCompletionContextRecursionTag.Value
            || !_options.Enable)
        {
            return null;
        }

        try
        {
            s_getCompletionContextRecursionTag.Value = true;

            if (_inputMethodDictionaryGroup is null
                || _inputMethodDictionaryGroup.IsDisposed)
            {
                _inputMethodDictionaryGroup = await InputMethodDictionaryGroupProvider.GetAsync();
            }

            var tasks = _otherAsyncCompletionSources.Select(m => m.GetCompletionContextAsync(session, trigger, triggerLocation, applicableToSpan, token)).ToArray();

            await Task.WhenAll(tasks);

            token.ThrowIfCancellationRequested();

            Func<string, bool> shouldProcessCheckDelegate = StringPreMatchUtil.GetPreCheckPredicate(_options.PreMatchType, _options.PreCheckRule);

            var allCompletionItems = tasks.SelectMany(static m => m.Status == TaskStatus.RanToCompletion && m.Result?.Items is not null ? m.Result.Items.AsEnumerable() : Array.Empty<CompletionItem>());

            var count = tasks.Sum(static m => m.Status == TaskStatus.RanToCompletion && m.Result?.Items is not null ? m.Result.Items.Length : 0);

            var itemBuffer = ArrayPool<CompletionItem>.Shared.Rent(count * 5);  //预留足够多的空间，避免字典过多导致的问题
            try
            {
                int bufferIndex = 0;
                allCompletionItems.AsParallel()
                                  .WithCancellation(token)
                                  .ForAll(m => CreateCompletionItemWithConvertion(m, shouldProcessCheckDelegate, itemBuffer, ref bufferIndex));

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
            s_getCompletionContextRecursionTag.Value = false;
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

    private void CreateCompletionItemWithConvertion(CompletionItem originCompletionItem, Func<string, bool> shouldProcessCheck, CompletionItem[] itemBuffer, ref int bufferIndex)
    {
        var originInsertText = originCompletionItem.InsertText;

        if (!shouldProcessCheck(originInsertText))
        {
            return;
        }

        var spellings = _inputMethodDictionaryGroup!.FindAll(originInsertText);

        if (spellings.Length == 0)
        {
            return;
        }

        if (_options.EnableFSharpSupport
            && originCompletionItem.Properties.TryGetProperty("RoslynCompletionItemData", out object data)
            && TryGetRoslynItemNameInCode(data, out var nameInCode))
        {
            originInsertText = nameInCode!;
        }

        if (_options.SingleWordsDisplay)
        {
            foreach (var spelling in spellings)
            {
                itemBuffer[Interlocked.Increment(ref bufferIndex) - 1] = CreateCompletionItem(originCompletionItem, originInsertText, spelling);
            }
        }
        else if (_options.EnableMultipleSpellings)
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
        var newCompletionItem = new CompletionItem(displayText: Format(_options.DisplayTextFormat, originCompletionItem.DisplayText, spelling),
                                                   source: this,
                                                   icon: originCompletionItem.Icon,
                                                   filters: s_chineseFilters,
                                                   suffix: Format(_options.DisplaySuffixFormat, originCompletionItem.Suffix, spelling),
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

        static string Format(string? format, string origin, string spellings)
        {
            if (string.IsNullOrEmpty(format))
            {
                return origin;
            }
            var builder = PooledStringBuilder.GetInstance();
            try
            {
                return builder.Builder.AppendFormat(format, origin, spellings).ToString();
            }
            finally
            {
                builder.Free();
            }
        }
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
