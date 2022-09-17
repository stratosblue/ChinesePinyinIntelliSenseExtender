using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ChinesePinyinIntelliSenseExtender.Options;
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

            var query = allCompletionItems.AsParallel().WithCancellation(token);

            query = GeneralOptions.Instance.CheckFirstCharOnly
                    ? query.Select(m => TryCreatePinyinCompletionItem(m, ChineseCheckUtil.StartWithChinese))
                    : query.Select(m => TryCreatePinyinCompletionItem(m, ChineseCheckUtil.ContainsChinese));

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

    #endregion impl
}
