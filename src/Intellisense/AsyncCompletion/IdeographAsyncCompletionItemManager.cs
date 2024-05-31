#nullable enable

using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Threading;
using ChinesePinyinIntelliSenseExtender.Options;
using ChinesePinyinIntelliSenseExtender.Util;
using Microsoft.VisualStudio.Language.Intellisense.AsyncCompletion;
using Microsoft.VisualStudio.Language.Intellisense.AsyncCompletion.Data;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.PatternMatching;

namespace ChinesePinyinIntelliSenseExtender.Intellisense.AsyncCompletion;

internal class IdeographAsyncCompletionItemManager(GeneralOptions options, IPatternMatcherFactory patternMatcherFactory) : IAsyncCompletionItemManager
{
    #region Private 字段

    /// <summary>
    /// 匹配缓存，当前没有移除？可能需要处理
    /// </summary>
    private readonly ConcurrentDictionary<(PreMatchType, StringPreCheckRule, string), string> _filterTextCache = [];

    private readonly GeneralOptions _options = options ?? throw new ArgumentNullException(nameof(options));

    private readonly IPatternMatcherFactory _patternMatcherFactory = patternMatcherFactory ?? throw new ArgumentNullException(nameof(patternMatcherFactory));

    private InputMethodDictionaryGroup? _inputMethodDictionaryGroup;

    #endregion Private 字段

    #region Public 方法

    public Task<ImmutableArray<CompletionItem>> SortCompletionListAsync(IAsyncCompletionSession session, AsyncCompletionSessionInitialDataSnapshot data, CancellationToken token)
    {
        if (!_options.Enable
            || _options.AsyncCompletionMode != AsyncCompletionMode.Experimental)
        {
            return Task.FromResult<ImmutableArray<CompletionItem>>(default);
        }

        //var st = ValueStopwatch.StartNew();
        var sortedItems = SortCompletionItem(data.InitialList).ToImmutableArray();
        //var t = st.Elapsed;
        //write(t);
        return Task.FromResult(sortedItems);
    }

    public async Task<FilteredCompletionModel> UpdateCompletionListAsync(IAsyncCompletionSession session, AsyncCompletionSessionDataSnapshot data, CancellationToken token)
    {
        if (!_options.Enable
            || _options.AsyncCompletionMode != AsyncCompletionMode.Experimental)
        {
            return default!;
        }

        //var st = ValueStopwatch.StartNew();
        //TimeSpan t;

        var view = session.TextView;
        // Filter by text
        var filterText = session.ApplicableToSpan.GetText(data.Snapshot);
        if (string.IsNullOrWhiteSpace(filterText)
            // 光标在 F# 的特殊标志符之后： ``aaa!`` |
            || char.IsWhiteSpace(filterText[filterText.Length - 1]))
        {
            // There is no text filtering. Just apply user filters, sort alphabetically and return.
            IReadOnlyList<CompletionItem> listFiltered = data.InitialSortedList;
            if (data.SelectedFilters.Any(n => n.IsSelected))
            {
                listFiltered = listFiltered.ParallelWhere(n => ShouldBeInCompletionList(n, data.SelectedFilters));
            }
            var listSorted = SortCompletionItem(listFiltered);
            var listHighlighted = listSorted.ParallelSelect(n => new CompletionItemWithHighlight(n)).ToImmutableArray();

            //t = st.Elapsed;
            //write(t, "short");
            return new FilteredCompletionModel(listHighlighted, 0, data.SelectedFilters);
        }

        var shouldProcessChecker = StringPreMatchUtil.GetPreCheckPredicate(_options.PreMatchType, _options.PreCheckRule);
        var inputMethodDictionaryGroup = await GetInputMethodDictionaryGroupAsync();

        string GetFilterText(string t)
        {
            var key = (_options.PreMatchType, _options.PreCheckRule, t);

            if (_filterTextCache.TryGetValue(key, out var r)) return r;
            if (!shouldProcessChecker.Check(t)) return AddToCacheAndReturn(t);

            var spellings = inputMethodDictionaryGroup.FindAll(t);
            var res = _options.EnableMultipleSpellings ? string.Join("/", spellings) : spellings[0];
            return AddToCacheAndReturn($"{t}/{res}");

            string AddToCacheAndReturn(string v)
            {
                _filterTextCache.TryAdd(key, v);
                return v;
            }
        }

        // Pattern matcher not only filters, but also provides a way to order the results by their match quality.
        // The relevant CompletionItem is match.Item1, its PatternMatch is match.Item2
        var patternMatcher = _patternMatcherFactory.CreatePatternMatcher(
            filterText,
            new PatternMatcherCreationOptions(System.Globalization.CultureInfo.CurrentCulture, PatternMatcherCreationFlags.IncludeMatchedSpans));

        var matches = data.InitialSortedList
            // Perform pattern matching
            .ParallelChoose(completionItem =>
            {
                var match = patternMatcher.TryMatch(GetFilterText(completionItem.FilterText));
                // Pick only items that were matched, unless length of filter text is 1
                return (filterText.Length == 1 || match.HasValue, (completionItem, match));
            });

        // See which filters might be enabled based on the typed code
        var textFilteredFilters = matches.SelectMany(n => n.completionItem.Filters).Distinct();

        // When no items are available for a given filter, it becomes unavailable
        var updatedFilters = ImmutableArray.CreateRange(data.SelectedFilters.Select(n => n.WithAvailability(textFilteredFilters.Contains(n.Filter))));

        // Filter by user-selected filters. The value on availableFiltersWithSelectionState conveys whether the filter is selected.
        var filterFilteredList = matches;
        if (data.SelectedFilters.Any(n => n.IsSelected))
        {
            filterFilteredList = matches.Where(n => ShouldBeInCompletionList(n.completionItem, data.SelectedFilters)).ToArray();
        }

        var bestMatch = filterFilteredList.OrderByDescending(n => n.match.HasValue).ThenBy(n => n.match).FirstOrDefault();
        var listWithHighlights = filterFilteredList.Select(n =>
        {
            ImmutableArray<Span> safeMatchedSpans = ImmutableArray<Span>.Empty;

            if (n.completionItem.DisplayText == n.completionItem.FilterText)
            {
                if (n.match.HasValue)
                {
                    safeMatchedSpans = n.match.Value.MatchedSpans;
                }
            }
            else
            {
                // Matches were made against FilterText. We are displaying DisplayText. To avoid issues, re-apply matches for these items
                var newMatchedSpans = patternMatcher.TryMatch(n.completionItem.DisplayText);
                if (newMatchedSpans.HasValue)
                {
                    safeMatchedSpans = newMatchedSpans.Value.MatchedSpans;
                }
            }

            if (safeMatchedSpans.IsDefaultOrEmpty)
            {
                return new CompletionItemWithHighlight(n.completionItem);
            }
            else
            {
                return new CompletionItemWithHighlight(n.completionItem, safeMatchedSpans);
            }
        }).ToImmutableArray();

        int selectedItemIndex = 0;
        if (data.DisplaySuggestionItem)
        {
            selectedItemIndex = -1;
        }
        else
        {
            for (int i = 0; i < listWithHighlights.Length; i++)
            {
                if (listWithHighlights[i].CompletionItem == bestMatch.completionItem)
                {
                    selectedItemIndex = i;
                    break;
                }
            }
        }

        //t = st.Elapsed;
        //write(t, "end");
        return new FilteredCompletionModel(listWithHighlights, selectedItemIndex, updatedFilters);
    }

    #endregion Public 方法

    #region Private 方法

    //private void write(TimeSpan t, string msg = "", [CallerMemberName] string n = "")
    //{
    //    using var s = File.AppendText("D:\\time.txt");
    //    s.WriteLine($"[{DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond}] {n}: {t.TotalMilliseconds}ms ({msg})");
    //}

    private static bool ShouldBeInCompletionList(CompletionItem item, ImmutableArray<CompletionFilterWithState> filtersWithState)
    {
        foreach (var filterWithState in filtersWithState.Where(n => n.IsSelected))
        {
            if (item.Filters.Any(n => n == filterWithState.Filter))
            {
                return true;
            }
        }
        return false;
    }

    private static IReadOnlyList<CompletionItem> SortCompletionItem(IReadOnlyList<CompletionItem> items)
    {
        bool CheckShouldSort()
        {
            return !(Parallel.For(1, items.Count, (i, p) =>
            {
                if (items[i - 1].SortText.CompareTo(items[i].SortText) > 0) p.Stop();
            }).IsCompleted);
        }

        if (CheckShouldSort()) { return items.AsParallel().OrderBy(i => i.SortText).ToArray(); }
        return items;
    }

    private async Task<InputMethodDictionaryGroup> GetInputMethodDictionaryGroupAsync()
    {
        if (_inputMethodDictionaryGroup is null
            || _inputMethodDictionaryGroup.IsDisposed)
        {
            _inputMethodDictionaryGroup = await InputMethodDictionaryGroupProvider.GetAsync();
        }
        return _inputMethodDictionaryGroup;
    }

    #endregion Private 方法
}
