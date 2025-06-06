using ChinesePinyinIntelliSenseExtender.Intellisense;

using Microsoft.VisualStudio.Language.Intellisense;

namespace ChinesePinyinIntelliSenseExtender.Util;

/// <summary>
/// <see cref="CompletionSet"/> 选择匹配项帮助类
/// </summary>
internal static class CompletionSetSelectBestMatchHelper
{
    #region Public 方法

    /// <summary>
    /// 使用输入 <paramref name="inputText"/> 匹配完成列表 <paramref name="completionList"/>
    /// </summary>
    /// <param name="completionList"></param>
    /// <param name="inputText"></param>
    /// <returns></returns>
    public static CompletionMatchResult? MatchCompletionList(IList<Completion> completionList, string inputText)
    {
        Completion? selectedCompletion = null;
        var selectedCompletionScore = 0;
        var charsMatchedCount = 0;

        var isUnique = false;
        var isSelected = false;

        foreach (var completion in completionList)
        {
            var currentMatchText = completion is IIdeographMatchableCompletion ideographMatchableCompletion
                                   ? ideographMatchableCompletion.MatchText
                                   : completion.DisplayText;

            if (string.IsNullOrEmpty(currentMatchText)
                || inputText.Length > currentMatchText!.Length)
            {
                continue;
            }

            var currentCompletionScore = InputTextMatchHelper.CalculateMatchScore(inputText, currentMatchText, out charsMatchedCount);

            if (currentCompletionScore > selectedCompletionScore)
            {
                selectedCompletionScore = currentCompletionScore;
                selectedCompletion = completion;
                isUnique = true;
                if (charsMatchedCount == inputText.Length
                    && selectedCompletionScore > 0)
                {
                    isSelected = true;
                }
            }
            else if (currentCompletionScore == selectedCompletionScore)
            {
                isUnique = false;
                //不提前退出，匹配所有项
                //if (isSelected)
                //{
                //    break;
                //}
            }
        }

        if (selectedCompletion is null)
        {
            return null;
        }

        return new(new CompletionSelectionStatus(selectedCompletion, isSelected, isUnique), charsMatchedCount);
    }

    /// <summary>
    /// 从 <paramref name="compareSelectionStatus"/> 和 <paramref name="baseSelectionStatus"/> 中选择一个更适合的 <see cref="CompletionSelectionStatus"/>
    /// </summary>
    /// <param name="compareSelectionStatus">用于比较的 <see cref="CompletionSelectionStatus"/></param>
    /// <param name="baseSelectionStatus">基类选择的 <see cref="CompletionSelectionStatus"/></param>
    /// <param name="inputText">输入的字符串</param>
    /// <returns></returns>
    public static CompletionSelectionStatus SelectSelectionStatus(CompletionSelectionStatus compareSelectionStatus, CompletionSelectionStatus baseSelectionStatus, string inputText)
    {
        if (baseSelectionStatus?.Completion is not { } baseSelectedCompletion
            || compareSelectionStatus.Completion is not IIdeographMatchableCompletion compareSelectedCompletion)    //基类未选择
        {
            return compareSelectionStatus;
        }
        if (baseSelectedCompletion is IIdeographMatchableCompletion) //基类选择了 IIdeographMatchableCompletion
        {
            return compareSelectionStatus;
        }

        if (ReferenceEquals(baseSelectedCompletion, compareSelectedCompletion)) //与基类选择相同
        {
            return baseSelectionStatus;
        }

        var baseSelectedCompletionScore = InputTextMatchHelper.CalculateMatchScore(inputText, baseSelectedCompletion.DisplayText, out var baseCharsMatchedCount);
        var compareSelectedCompletionScore = InputTextMatchHelper.CalculateMatchScore(inputText, compareSelectedCompletion.MatchText, out var compareCharsMatchedCount);

        if (compareSelectedCompletionScore > baseSelectedCompletionScore)
        {
            return compareSelectionStatus;
        }
        return baseSelectionStatus;
    }

    #endregion Public 方法

    #region Public 类

    public sealed class CompletionMatchResult(CompletionSelectionStatus selectionStatus, int charsMatchedCount)
    {
        #region Public 属性

        public int CharsMatchedCount { get; set; } = charsMatchedCount;

        public CompletionSelectionStatus SelectionStatus { get; set; } = selectionStatus;

        #endregion Public 属性
    }

    #endregion Public 类
}
