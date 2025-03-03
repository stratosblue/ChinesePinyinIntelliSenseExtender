namespace ChinesePinyinIntelliSenseExtender.Util;

/// <summary>
/// 输入文本匹配帮助类
/// </summary>
internal static class InputTextMatchHelper
{
    #region Public 方法

    /// <summary>
    /// 计算匹配分
    /// </summary>
    /// <param name="inputText">输入文本</param>
    /// <param name="matchText">目标文本</param>
    /// <param name="charsMatchedCount">字符匹配数量</param>
    /// <returns>匹配分</returns>
    public static int CalculateMatchScore(string inputText, string matchText, out int charsMatchedCount)
    {
        // 通过计算 inputText 对应 matchText 的匹配程度得出其匹配分
        // --- 未测试 ---
        // 分数排序预期应该如下顺序
        // inputText 完全匹配 matchText
        // inputText 在 matchText 中找到更多的大写匹配，eg: hdn -> HaoDeNe 分数应大于 HaoDeng
        // inputText 在 matchText 中找到更多连续匹配，eg: ang -> YangPin 分数应大于 Haineng
        // inputText 与 matchText 首字母匹配，eg: Hang -> HaoYang 分数应大于 NiHaoYang

        const int InitSocreRewards = 1; //初始分数奖励
        const int UpperMatchSocre = 5;  //大写匹配分数

        var currentCompletionScore = -1;
        charsMatchedCount = 0;

        var inputLength = inputText.Length;
        var matchLength = matchText.Length;

        if (inputLength > matchLength)
        {
            return currentCompletionScore;
        }

        if (matchText.StartsWith("#", StringComparison.Ordinal))    //以 # 起始的特殊输入，完全匹配
        {
            if (inputLength > matchLength - 1)
            {
                return currentCompletionScore;
            }

            int i = 1, j = 0;
            for (; i < matchText.Length && j < inputText.Length; i++, j++)
            {
                if (matchText[i] != inputText[j])
                {
                    return currentCompletionScore;
                }
            }
            return i == matchText.Length - 1
                   ? i * 1000 * UpperMatchSocre
                   : i * 100 * UpperMatchSocre;
        }

        //下方逻辑是从 CompletionSet 中 copy 后改的

        var inputIndex = 0;
        var matchIndex = 0;
        var scoreRewards = InitSocreRewards;  //分数奖励

        for (; inputIndex < inputLength; inputIndex++)
        {
            var inputChar = char.ToLowerInvariant(inputText[inputIndex]);

            for (; matchIndex < matchLength;)
            {
                var originMatchChar = matchText[matchIndex++];
                var matchChar = char.ToLowerInvariant(originMatchChar);

                if (inputChar != matchChar)
                {
                    if (!char.IsPunctuation(originMatchChar))   //非符号匹配，重置连续匹配奖励分
                    {
                        scoreRewards = InitSocreRewards;
                    }
                    continue;
                }

                if (char.IsUpper(originMatchChar))  //大写字母匹配
                {
                    currentCompletionScore += UpperMatchSocre + scoreRewards;
                }
                else
                {
                    currentCompletionScore += scoreRewards;   //普通匹配
                }

                if (matchIndex == 0)    //首字母匹配
                {
                    currentCompletionScore += UpperMatchSocre;
                }

                scoreRewards++; //增加匹配分
                charsMatchedCount++;
                break;
            }
        }

        if (currentCompletionScore > 0)
        {
            if (charsMatchedCount != inputLength)   //输入未完全匹配，分数置空
            {
                currentCompletionScore = -1;
            }
            else if (scoreRewards - 1 == inputLength)   //连续完整匹配,分数倍率提升
            {
                if (charsMatchedCount == matchLength)  //完整匹配目标项
                {
                    currentCompletionScore *= 12 * inputLength;
                }
                else
                {
                    currentCompletionScore *= 10 * inputLength;
                }
            }
            else //分数根据匹配长度倍率提升
            {
                currentCompletionScore = (int)(currentCompletionScore * (charsMatchedCount * 10.0 / matchLength));
            }
        }

        return currentCompletionScore;
    }

    #endregion Public 方法
}
