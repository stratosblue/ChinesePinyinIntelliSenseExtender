using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using ChinesePinyinIntelliSenseExtender.Options;
using ChinesePinyinIntelliSenseExtender.Util;
using Microsoft.VisualStudio.Core.Imaging;
using Microsoft.VisualStudio.Imaging;
using Microsoft.VisualStudio.Language.Intellisense.AsyncCompletion;
using Microsoft.VisualStudio.Language.Intellisense.AsyncCompletion.Data;
using Microsoft.VisualStudio.Text;

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

    private readonly IEnumerable<IAsyncCompletionSource> _otherAsyncCompletionSources;

    #endregion Private 字段

    #region Public 构造函数

    public PinyinAsyncCompletionSource(IEnumerable<IAsyncCompletionSource> otherAsyncCompletionSources)
    {
        _otherAsyncCompletionSources = otherAsyncCompletionSources;
    }

    #endregion Public 构造函数

    #region Public 方法

    public async Task<CompletionContext> GetCompletionContextAsync(IAsyncCompletionSession session, CompletionTrigger trigger, SnapshotPoint triggerLocation, SnapshotSpan applicableToSpan, CancellationToken token)
    {
        if (s_getCompletionContextRecursionTag.Value
            || !GeneralOptions.Instance.Enable)
        {
            return null;
        }
        try
        {
            s_getCompletionContextRecursionTag.Value = true;

            var tasks = _otherAsyncCompletionSources.Select(m => m.GetCompletionContextAsync(session, trigger, triggerLocation, applicableToSpan, token)).ToArray();

            await Task.WhenAll(tasks);

            token.ThrowIfCancellationRequested();

            var allCompletionItems = tasks.SelectMany(static m => m.Status == TaskStatus.RanToCompletion && m.Result?.Items is not null ? m.Result.Items.AsEnumerable() : Array.Empty<CompletionItem>());

            var table = await CharacterTable.MakeTableAsync(@"C:\Users\Liu00\AppData\Roaming\Rime\pinyin_simp.dict.yaml", '\t');

            Func<string, bool> method = GeneralOptions.Instance.CheckFirstCharOnly ? ChineseCheckUtil.StartWithChinese : ChineseCheckUtil.ContainsChinese;

            var query = allCompletionItems.AsParallel().WithCancellation(token);

            query = query.SelectMany(m => CreateCompletionItemWithConvertion(m, method, table));

            var pinyinCompletions = query.Where(static m => m is not null)
                                         .ToImmutableArray();

            return pinyinCompletions.Length == 0
                   ? null
                   : new CompletionContext(pinyinCompletions);
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

    private CompletionItem TryCreatePinyinCompletionItem(CompletionItem originCompletionItem, Func<string, bool> shouldProcessCheck)
    {
        var originInsertText = originCompletionItem.InsertText;

        if (!shouldProcessCheck(originInsertText))
        {
            return null;
        }

        var pinyin = ChineseCharPinyinConverter.Convert(originInsertText);

        if (string.Equals(originInsertText, pinyin, StringComparison.Ordinal))
        {
            return null;
        }

        var appendCompletionItem = new CompletionItem(displayText: $"{originCompletionItem.DisplayText} [{pinyin}]",
                                                      source: this,
                                                      icon: originCompletionItem.Icon,
                                                      filters: s_chineseFilters,
                                                      suffix: originCompletionItem.Suffix,
                                                      insertText: originInsertText,
                                                      sortText: pinyin,
                                                      filterText: pinyin,
                                                      automationText: originCompletionItem.AutomationText,
                                                      attributeIcons: originCompletionItem.AttributeIcons,
                                                      commitCharacters: originCompletionItem.CommitCharacters,
                                                      applicableToSpan: originCompletionItem.ApplicableToSpan,
                                                      isCommittedAsSnippet: originCompletionItem.IsCommittedAsSnippet,
                                                      isPreselected: originCompletionItem.IsPreselected);

        appendCompletionItem.Properties.AddProperty(this, originCompletionItem);

        return appendCompletionItem;
    }

    ConditionalWeakTable<string, IEnumerable<string>> t = new();

    private IEnumerable<CompletionItem> CreateCompletionItemWithConvertion(CompletionItem originCompletionItem, Func<string, bool> shouldProcessCheck, CharacterTable table)
    {
        var originInsertText = originCompletionItem.InsertText;

        if (!shouldProcessCheck(originInsertText))
        {
            return Enumerable.Empty<CompletionItem>();
        }

        if (!t.TryGetValue(originInsertText, out IEnumerable<string> spells))
        {
            spells = Enumerable.Repeat(string.Empty, 1);
            for (int k = 0; k < originInsertText.Length; k++)
            {
                var item = originInsertText[k];
                spells =
                    from i in spells
                    from j in table.GetSpells(item.ToString())
                    select i + j;
                t.Add(originInsertText.Substring(0, k + 1), spells);
            }
        }

        if (spells.Count() == 1 && string.Equals(originInsertText, spells.First(), StringComparison.Ordinal))
        {
            return Enumerable.Empty<CompletionItem>();
        }

        // 获取 F# 中特殊标识符（包含空格等特殊字符）的实际写法。F# 并没有将真正要写到代码里的内容存到 CompletionItem.InsertText 里，而是放在了
        // CompletionItem.Properties["RoslynCompletionItemData"].Value.Properties["NameInCode"] 中，因此需要用反射来获取。
        // 下面是 vs F# 扩展插入提示项的方法：
        // https://github.com/dotnet/fsharp/blob/main/vsintegration/src/FSharp.Editor/Completion/CompletionProvider.fs#L250
        if (originCompletionItem.Properties.TryGetProperty("RoslynCompletionItemData", out object data))
        {
            var dataType = data.GetType();
            if (dataType.FullName == "Microsoft.CodeAnalysis.Editor.Implementation.IntelliSense.AsyncCompletion.CompletionItemData")
            {
                var RoslynItem = dataType.GetProperty("RoslynItem").GetMethod.Invoke(data, Array.Empty<object>());
                var properties = (ImmutableDictionary<string, string>)RoslynItem.GetType().GetProperty("Properties").GetMethod.Invoke(RoslynItem, Array.Empty<object>());
                if (properties.TryGetValue("NameInCode", out var code))
                {
                    originInsertText = code;
                }
            }
        }

        return spells.AsParallel().Select(spell =>
        {
            CompletionItem appendCompletionItem = new(
                displayText: $"{originCompletionItem.DisplayText} [{spell}]",
                source: this,
                icon: originCompletionItem.Icon,
                filters: s_chineseFilters,
                suffix: originCompletionItem.Suffix,
                insertText: originInsertText,
                sortText: spell,
                filterText: spell,
                automationText: originCompletionItem.AutomationText,
                attributeIcons: originCompletionItem.AttributeIcons,
                commitCharacters: originCompletionItem.CommitCharacters,
                applicableToSpan: originCompletionItem.ApplicableToSpan,
                isCommittedAsSnippet: originCompletionItem.IsCommittedAsSnippet,
                isPreselected: originCompletionItem.IsPreselected);

            appendCompletionItem.Properties.AddProperty(this, originCompletionItem);
            return appendCompletionItem;
        });
    }


    #endregion impl
}
