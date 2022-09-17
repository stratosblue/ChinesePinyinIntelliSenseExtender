using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

using ChinesePinyinIntelliSenseExtender.Options;

using Microsoft.VisualStudio.Language.Intellisense;

namespace ChinesePinyinIntelliSenseExtender;

internal class PinyinCompletionSource : ICompletionSource
{
    #region Private 字段

    /// <summary>
    /// <see cref="AugmentCompletionSession(ICompletionSession, IList{CompletionSet})"/> 递归标记
    /// </summary>
    private static readonly AsyncLocal<bool> s_getCompletionContextRecursionTag = new();

    #endregion Private 字段

    #region Public 方法

    public void AugmentCompletionSession(ICompletionSession session, IList<CompletionSet> completionSets)
    {
        if (s_getCompletionContextRecursionTag.Value
            || !GeneralOptions.Instance.Enable)
        {
            return;
        }
        try
        {
            s_getCompletionContextRecursionTag.Value = true;

            for (int setIndex = 0; setIndex < completionSets.Count; setIndex++)
            {
                var completionSet = completionSets[setIndex];

                var query = completionSet.Completions.AsQueryable();

                query = GeneralOptions.Instance.CheckFirstCharOnly
                        ? query.Select(m => TryCreatePinyinCompletion(m, ChineseCheckUtil.StartWithChinese))
                        : query.Select(m => TryCreatePinyinCompletion(m, ChineseCheckUtil.ContainsChinese));

                var pinyinCompletions = query.Where(static m => m != null)
                                             .ToArray();

                if (pinyinCompletions.Length > 0)
                {
                    var pinyinCompletionSet = CreateNewCompletionSet(completionSet, completionSet.Completions.Concat(pinyinCompletions));

                    completionSets.RemoveAt(setIndex);
                    completionSets.Insert(setIndex, pinyinCompletionSet);
                }
            }
        }
        finally
        {
            s_getCompletionContextRecursionTag.Value = false;
        }
    }

    public void Dispose()
    {
    }

    #endregion Public 方法

    #region impl

    private static CompletionSet CreateNewCompletionSet(CompletionSet completionSet, IEnumerable<Completion> completions)
    {
        if (completionSet is CompletionSet2 completionSet2)
        {
            return new CompletionSet2(completionSet2.Moniker, completionSet2.DisplayName, completionSet2.ApplicableTo, completions, completionSet2.CompletionBuilders, completionSet2.Filters);
        }
        else
        {
            return new CompletionSet(completionSet.Moniker, completionSet.DisplayName, completionSet.ApplicableTo, completions, completionSet.CompletionBuilders);
        }
    }

    private Completion? TryCreatePinyinCompletion(Completion originCompletion, Func<string, bool> shouldProcessCheck)
    {
        var originInsertText = originCompletion.InsertionText;

        if (!shouldProcessCheck(originInsertText))
        {
            return null;
        }

        var pinyin = ChineseCharPinyinConverter.Convert(originInsertText);

        if (string.Equals(originInsertText, pinyin, StringComparison.Ordinal))
        {
            return null;
        }

        var displayText = $"{pinyin} {{{originInsertText}}}";

        Completion pinyinCompletion;

        if (originCompletion is Completion4 completion4)
        {
            pinyinCompletion = new Completion4(displayText: displayText,
                                               insertionText: completion4.InsertionText,
                                               description: GetDescription(completion4),
                                               iconMoniker: completion4.IconMoniker,
                                               iconAutomationText: completion4.IconAutomationText,
                                               attributeIcons: completion4.AttributeIcons?.OfType<CompletionIcon2>(),
                                               suffix: completion4.Suffix);
        }
        else if (originCompletion is Completion3 completion3)
        {
            pinyinCompletion = new Completion3(displayText: displayText,
                                               insertionText: completion3.InsertionText,
                                               description: GetDescription(completion3),
                                               iconMoniker: completion3.IconMoniker,
                                               iconAutomationText: completion3.IconAutomationText,
                                               attributeIcons: completion3.AttributeIcons.OfType<CompletionIcon2>());
        }
        else if (originCompletion is Completion2 completion2)
        {
            pinyinCompletion = new Completion2(displayText: displayText,
                                               insertionText: completion2.InsertionText,
                                               description: GetDescription(completion2),
                                               iconSource: completion2.IconSource,
                                               iconAutomationText: completion2.IconAutomationText,
                                               attributeIcons: completion2.AttributeIcons);
        }
        else
        {
            pinyinCompletion = new Completion(displayText: displayText,
                                              insertionText: originCompletion.InsertionText,
                                              description: GetDescription(originCompletion),
                                              iconSource: originCompletion.IconSource,
                                              iconAutomationText: originCompletion.IconAutomationText);
        }

        return pinyinCompletion;

        static string GetDescription(Completion completion)
        {
            try
            {
                return completion.Description;
            }
            catch { }
            return null;
        }
    }

    #endregion impl
}
