#nullable enable

using System.Buffers;
using ChinesePinyinIntelliSenseExtender.Internal;
using ChinesePinyinIntelliSenseExtender.Options;
using ChinesePinyinIntelliSenseExtender.Util;
using Microsoft.VisualStudio.Language.Intellisense;

namespace ChinesePinyinIntelliSenseExtender.Intellisense.SyncCompletion;

internal class IdeographCompletionSource : CompletionSourceBase, ICompletionSource
{
    #region Public 构造函数

    public IdeographCompletionSource(GeneralOptions options) : base(options)
    {
    }

    #endregion Public 构造函数

    #region Public 方法

    public void AugmentCompletionSession(ICompletionSession session, IList<CompletionSet> completionSets)
    {
        if (!Options.Enable
            || !Options.EnableSyncCompletionSupport)
        {
            return;
        }

        for (int setIndex = 0; setIndex < completionSets.Count; setIndex++)
        {
            var completionSet = completionSets[setIndex];

            var allCompletions = completionSet.Completions;

            var inputMethodDictionaryGroup = GetInputMethodDictionaryGroup();

            var shouldProcessCheckDelegate = StringPreMatchUtil.GetPreCheckPredicate(Options.PreMatchType, Options.PreCheckRule);

            var itemBuffer = ArrayPool<Completion>.Shared.Rent(allCompletions.Count * 5);  //预留足够多的空间，避免字典过多导致的问题
            try
            {
                int bufferIndex = 0;
                foreach (var completion in allCompletions)
                {
                    CreateCompletionWithConvertion(completion, inputMethodDictionaryGroup, shouldProcessCheckDelegate, itemBuffer, ref bufferIndex);
                }

                if (bufferIndex > 0)
                {
                    foreach (var completion in allCompletions)
                    {
                        itemBuffer[bufferIndex++] = CloneCompletion(completion);
                    }

                    var processedCompletionSet = CreateNewCompletionSet(completionSet, new ArrayBaseEnumerator<Completion>(itemBuffer, 0, bufferIndex));
                    completionSets.RemoveAt(setIndex);
                    completionSets.Insert(setIndex, processedCompletionSet);
                }
            }
            finally
            {
                ArrayPool<Completion>.Shared.Return(itemBuffer);
            }
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
            return new IdeographCompletionSet2(completionSet2.Moniker, completionSet2.DisplayName, completionSet2.ApplicableTo, completions, completionSet2.CompletionBuilders, completionSet2.Filters);
        }
        else
        {
            return new IdeographCompletionSet(completionSet.Moniker, completionSet.DisplayName, completionSet.ApplicableTo, completions, completionSet.CompletionBuilders);
        }
    }

    private Completion CloneCompletion(Completion originCompletion)
    {
        return originCompletion switch
        {
            Completion4 completion4 => completion4.Suffix?.Length > 0
                                       ? new IdeographCompletion4(displayText: completion4.DisplayText, suffix: completion4.Suffix, matchText: null, origin: completion4)
                                       : new IdeographCompletion4(displayText: completion4.DisplayText, matchText: null, origin: completion4),
            Completion3 completion3 => new IdeographCompletion3(displayText: completion3.DisplayText, matchText: null, origin: completion3),
            Completion2 completion2 => new IdeographCompletion2(displayText: completion2.DisplayText, matchText: null, origin: completion2),
            _ => new IdeographCompletion(displayText: originCompletion.DisplayText, matchText: null, origin: originCompletion),
        };
    }

    private Completion CreateCompletion(Completion originCompletion, string originInsertText, string spelling)
    {
        var displayText = FormatString(Options.SyncCompletionDisplayTextFormat, spelling, originInsertText);

        return originCompletion switch
        {
            Completion4 completion4 => completion4.Suffix?.Length > 0
                                       ? new IdeographCompletion4(displayText: displayText, suffix: completion4.Suffix, matchText: spelling, origin: completion4)
                                       : new IdeographCompletion4(displayText: displayText, matchText: spelling, origin: completion4),
            Completion3 completion3 => new IdeographCompletion3(displayText: displayText, matchText: spelling, origin: completion3),
            Completion2 completion2 => new IdeographCompletion2(displayText: displayText, matchText: spelling, origin: completion2),
            _ => new IdeographCompletion(displayText: displayText, matchText: spelling, origin: originCompletion),
        };
    }

    private void CreateCompletionWithConvertion(Completion originCompletion, InputMethodDictionaryGroup inputMethodDictionaryGroup, Func<string, bool> shouldProcessCheck, Completion[] itemBuffer, ref int bufferIndex)
    {
        var originInsertText = originCompletion.InsertionText;

        if (string.IsNullOrEmpty(originInsertText)
            || !shouldProcessCheck(originInsertText))
        {
            return;
        }

        var spellings = inputMethodDictionaryGroup.FindAll(originInsertText);

        if (spellings.Length == 0)
        {
            return;
        }

        if (Options.SingleWordsDisplay)
        {
            foreach (var spelling in spellings)
            {
                itemBuffer[bufferIndex++] = CreateCompletion(originCompletion, originInsertText, spelling);
            }
        }
        else if (Options.EnableMultipleSpellings)
        {
            itemBuffer[bufferIndex++] = CreateCompletion(originCompletion, originInsertText, string.Join("/", spellings));
        }
        else
        {
            itemBuffer[bufferIndex++] = CreateCompletion(originCompletion, originInsertText, spellings[0]);
        }
    }

    #endregion impl
}
