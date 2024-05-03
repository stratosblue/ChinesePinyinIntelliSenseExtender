#nullable enable

using System.Globalization;
using ChinesePinyinIntelliSenseExtender.Util;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;

namespace ChinesePinyinIntelliSenseExtender.Intellisense.SyncCompletion;

internal sealed class InnerIdeographCompletionSet : IIdeographCompletionSet
{
    #region Private 字段

    private string? _currentInputText;

    private int _currentInputTextVersionNumber = -1;

    #endregion Private 字段

    #region Public 属性

    public ITrackingSpan ApplicableTo => CompletionSet.ApplicableTo;

    public FilteredObservableCollection<Completion> CompletionBuilders { get; }

    public FilteredObservableCollection<Completion> Completions { get; }

    public CompletionSet CompletionSet { get; }

    #endregion Public 属性

    #region Private 属性

    private string? CurrentInputText
    {
        get
        {
            if (ApplicableTo != null)
            {
                ITextSnapshot currentSnapshot = ApplicableTo.TextBuffer.CurrentSnapshot;
                if (_currentInputText == null
                    || _currentInputTextVersionNumber != currentSnapshot.Version.VersionNumber)
                {
                    _currentInputText = ApplicableTo.GetText(currentSnapshot);
                    _currentInputTextVersionNumber = currentSnapshot.Version.VersionNumber;
                }
            }

            return _currentInputText;
        }
    }

    #endregion Private 属性

    #region Public 构造函数

    public InnerIdeographCompletionSet(CompletionSet completionSet, FilteredObservableCollection<Completion> completions, FilteredObservableCollection<Completion> completionBuilders)
    {
        Completions = completions ?? throw new ArgumentNullException(nameof(completions));
        CompletionBuilders = completionBuilders ?? throw new ArgumentNullException(nameof(completionBuilders));
        CompletionSet = completionSet ?? throw new ArgumentNullException(nameof(completionSet));
    }

    #endregion Public 构造函数

    #region Private 方法

    private bool DoesCompletionMatchApplicabilityText(Completion completion, string inputText)
    {
        if (completion is IIdeographCompletion ideographCompletion
            && ideographCompletion.MatchText is not null)
        {
            return InputTextMatchHelper.CalculateMatchScore(inputText, ideographCompletion.MatchText, out _) > 0;
        }
        else
        {
            return completion.DisplayText.StartsWith(inputText, true, CultureInfo.CurrentCulture);
        }
    }

    #endregion Private 方法

    #region Public 方法

    public void Filter()
    {
        var inputText = CurrentInputText!;
        if (string.IsNullOrEmpty(inputText))
        {
            Completions.StopFiltering();
            CompletionBuilders.StopFiltering();
            return;
        }

        Completions.Filter(m => DoesCompletionMatchApplicabilityText(m, inputText));
        CompletionBuilders.Filter(m => DoesCompletionMatchApplicabilityText(m, inputText));
    }

    public IReadOnlyList<Span> GetHighlightedSpansInDisplayText(string displayText)
    {
        var inputText = CurrentInputText!;
        if (string.IsNullOrEmpty(inputText))
        {
            return [];
        }

        try
        {
            var result = new List<Span>(inputText.Length);

            var start = 0;
            var length = 0;
            var inputTextIndex = 0;
            var displayTextIndex = 0;

            for (; inputTextIndex < inputText.Length; inputTextIndex++)
            {
                var inputChar = char.ToLowerInvariant(inputText[inputTextIndex]);

                for (; displayTextIndex < displayText.Length;)
                {
                    var matchChar = char.ToLowerInvariant(displayText[displayTextIndex++]);
                    if (inputChar == matchChar)
                    {
                        length++;
                        break;
                    }

                    if (length > 0)
                    {
                        result.Add(new(start, length));
                        length = 0;
                    }

                    start = displayTextIndex;
                }
            }

            if (length > 0)
            {
                result.Add(new(start, length));
            }
            return result;
        }
        catch
        {
            // 忽略异常
        }
        return [];
    }

    public void SelectBestMatch()
    {
        var inputText = CurrentInputText!;
        if (string.IsNullOrWhiteSpace(inputText))
        {
            return;
        }

        //下方逻辑是从 CompletionSet 中 copy 后改的

        var completionsMatchResult = CompletionSetSelectBestMatchHelper.MatchCompletionList(Completions, inputText);
        var completionBuildersMatchResult = CompletionSetSelectBestMatchHelper.MatchCompletionList(CompletionBuilders, inputText);

        var completionBuildersCharsMatchedCount = 0;
        if (completionBuildersMatchResult != null)
        {
            completionBuildersCharsMatchedCount = completionBuildersMatchResult.CharsMatchedCount + (completionBuildersMatchResult.SelectionStatus.IsSelected ? 1 : 0) + (completionBuildersMatchResult.SelectionStatus.IsUnique ? 1 : 0);
        }

        var completionsCharsMatchedCount = 0;
        if (completionsMatchResult != null)
        {
            completionsCharsMatchedCount = completionsMatchResult.CharsMatchedCount + (completionsMatchResult.SelectionStatus.IsSelected ? 1 : 0) + (completionsMatchResult.SelectionStatus.IsUnique ? 1 : 0);
        }

        var selectionStatus = CompletionSet.SelectionStatus;

        if (completionBuildersCharsMatchedCount > completionsCharsMatchedCount
            && completionBuildersMatchResult != null)
        {
            try
            {
                selectionStatus = CompletionSetSelectBestMatchHelper.SelectSelectionStatus(completionBuildersMatchResult.SelectionStatus, selectionStatus, inputText);
            }
            catch (ArgumentException) { }
        }
        else if (completionsMatchResult != null)
        {
            try
            {
                selectionStatus = CompletionSetSelectBestMatchHelper.SelectSelectionStatus(completionsMatchResult.SelectionStatus, selectionStatus, inputText);
            }
            catch (ArgumentException) { }
        }
        else if (Completions.Count > 0)
        {
            if (Completions.Contains(selectionStatus.Completion))
            {
                selectionStatus = new CompletionSelectionStatus(selectionStatus.Completion, isSelected: false, isUnique: false);
            }
            else
            {
                selectionStatus = new CompletionSelectionStatus(Completions[0], isSelected: false, isUnique: false);
            }
        }
        else if (CompletionBuilders.Count > 0)
        {
            if (CompletionBuilders.Contains(selectionStatus.Completion))
            {
                selectionStatus = new CompletionSelectionStatus(selectionStatus.Completion, isSelected: false, isUnique: false);
            }
            else
            {
                selectionStatus = new CompletionSelectionStatus(CompletionBuilders[0], isSelected: false, isUnique: false);
            }
        }
        else
        {
            selectionStatus = new CompletionSelectionStatus(null, isSelected: false, isUnique: false);
        }

        CompletionSet.SelectionStatus = selectionStatus;
    }

    #endregion Public 方法
}
