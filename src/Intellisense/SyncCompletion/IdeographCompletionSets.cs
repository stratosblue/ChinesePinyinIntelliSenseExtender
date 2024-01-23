using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;

namespace ChinesePinyinIntelliSenseExtender.Intellisense.SyncCompletion;

internal class IdeographCompletionSet : CompletionSet, IIdeographCompletionSet
{
    #region Public 构造函数

    public IdeographCompletionSet(string moniker, string displayName, ITrackingSpan applicableTo, IEnumerable<Completion> completions, IEnumerable<Completion> completionBuilders) : base(moniker, displayName, applicableTo, completions, completionBuilders)
    {
    }

    #endregion Public 构造函数
}

internal class IdeographCompletionSet2 : CompletionSet2, IIdeographCompletionSet
{
    #region Public 构造函数

    public IdeographCompletionSet2(string moniker, string displayName, ITrackingSpan applicableTo, IEnumerable<Completion> completions, IEnumerable<Completion> completionBuilders, IReadOnlyList<IIntellisenseFilter> filters) : base(moniker, displayName, applicableTo, completions, completionBuilders, filters)
    {
    }

    #endregion Public 构造函数
}
