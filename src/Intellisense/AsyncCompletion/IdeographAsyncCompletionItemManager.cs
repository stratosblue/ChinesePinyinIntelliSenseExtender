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

internal class IdeographAsyncCompletionItemManager(IPatternMatcherFactory _patternMatcherFactory) : IAsyncCompletionItemManager
{
    //private async Task write(TimeSpan t, string msg = "", [CallerMemberName] string n = "")
    //{
    //    using var s = File.AppendText("D:\\time.txt");
    //    await s.WriteLineAsync($"[{DateTime.Now}] {n}: {t} ({msg})");
    //}

    private InputMethodDictionaryGroup? _inputMethodDictionaryGroup;
    private readonly ConcurrentDictionary<(PreMatchType, StringPreCheckRule, string), string> _cache = [];
    private GeneralOptions Options => GeneralOptions.Instance;

    private async Task<InputMethodDictionaryGroup> GetInputMethodDictionaryGroupAsync()
    {
        if (_inputMethodDictionaryGroup is null
            || _inputMethodDictionaryGroup.IsDisposed)
        {
            _inputMethodDictionaryGroup = await InputMethodDictionaryGroupProvider.GetAsync();
        }
        return _inputMethodDictionaryGroup;
    }

    private string GetFilterText(string t, IPreCheckPredicate shouldProcessChecker, InputMethodDictionaryGroup inputMethodDictionaryGroup)
    {
        var key = (Options.PreMatchType, Options.PreCheckRule, t);

        if (_cache.TryGetValue(key, out var r)) return r;
        if (!shouldProcessChecker.Check(t)) AddToCacheAndReturn(t);

        var spellings = inputMethodDictionaryGroup.FindAll(t);
        var res = Options.EnableMultipleSpellings ? string.Join("/", spellings) : spellings[0];
        return AddToCacheAndReturn($"{t}/{res}");

        string AddToCacheAndReturn(string v)
        {
            _cache.TryAdd(key, v);
            return v;
        }
    }

    public Task<ImmutableArray<CompletionItem>> SortCompletionListAsync(IAsyncCompletionSession session, AsyncCompletionSessionInitialDataSnapshot data, CancellationToken token)
    {
        //var st = ValueStopwatch.StartNew();
        var sortedItems = data.InitialList.ToArray().OrderBy(i => i.SortText).ToImmutableArray();
        //var t = st.Elapsed;
        //await write(t);
        //return sortedItems;
        return Task.FromResult(sortedItems);
    }

    public async Task<FilteredCompletionModel> UpdateCompletionListAsync(IAsyncCompletionSession session, AsyncCompletionSessionDataSnapshot data, CancellationToken token)
    {
        //var st = ValueStopwatch.StartNew();

        var view = session.TextView;
        // Filter by text
        var filterText = session.ApplicableToSpan.GetText(data.Snapshot);
        if (string.IsNullOrWhiteSpace(filterText))
        {
            // There is no text filtering. Just apply user filters, sort alphabetically and return.
            IEnumerable<CompletionItem> listFiltered = data.InitialSortedList.AsParallel();
            if (data.SelectedFilters.Any(n => n.IsSelected))
            {
                listFiltered = listFiltered.Where(n => ShouldBeInCompletionList(n, data.SelectedFilters));
            }
            var listSorted = listFiltered.OrderBy(n => n.SortText);
            var listHighlighted = listSorted.Select(n => new CompletionItemWithHighlight(n)).ToImmutableArray();

            //var t2 = st.Elapsed;
            //await write(t2, "short");
            return new FilteredCompletionModel(listHighlighted, 0, data.SelectedFilters);
        }

        var Options = GeneralOptions.Instance;
        var shouldProcessChecker = StringPreMatchUtil.GetPreCheckPredicate(Options.PreMatchType, Options.PreCheckRule);
        var inputMethodDictionaryGroup = await GetInputMethodDictionaryGroupAsync();

        // Pattern matcher not only filters, but also provides a way to order the results by their match quality.
        // The relevant CompletionItem is match.Item1, its PatternMatch is match.Item2
        var patternMatcher = _patternMatcherFactory.CreatePatternMatcher(
            filterText,
            new PatternMatcherCreationOptions(System.Globalization.CultureInfo.CurrentCulture, PatternMatcherCreationFlags.IncludeMatchedSpans));

        var matches = data.InitialSortedList
            // Perform pattern matching
            .ParallelChoose(completionItem =>
            {
                var n = patternMatcher.TryMatch(GetFilterText(completionItem.FilterText, shouldProcessChecker, inputMethodDictionaryGroup));
                // Pick only items that were matched, unless length of filter text is 1
                return (filterText.Length == 1 || n.HasValue, (completionItem, n));
            })
            .ToArray();

        //var t = st.Elapsed;
        //await write(t);

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

        var bestMatch = filterFilteredList.OrderByDescending(n => n.Item2.HasValue).ThenBy(n => n.Item2).FirstOrDefault();
        var listWithHighlights = filterFilteredList.Select(n =>
        {
            ImmutableArray<Span> safeMatchedSpans = ImmutableArray<Span>.Empty;
            if (n.completionItem.DisplayText == n.completionItem.FilterText)
            {
                if (n.Item2.HasValue)
                {
                    safeMatchedSpans = n.Item2.Value.MatchedSpans;
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
        //await write(t, "end");
        return new FilteredCompletionModel(listWithHighlights, selectedItemIndex, updatedFilters);
    }


    private static bool ShouldBeInCompletionList(
        CompletionItem item,
        ImmutableArray<CompletionFilterWithState> filtersWithState)
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
}
