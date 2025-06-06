using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;

namespace ChinesePinyinIntelliSenseExtender.Intellisense.SyncCompletion;

internal class IdeographCompletionSet : CompletionSet, IIdeographCompletionSet
{
    #region Private 字段

    private readonly InnerIdeographCompletionSet _innerIdeographCompletionSet;

    #endregion Private 字段

    #region Public 属性

    public override IList<Completion> CompletionBuilders => _innerIdeographCompletionSet.CompletionBuilders;

    public override IList<Completion> Completions => _innerIdeographCompletionSet.Completions;

    #endregion Public 属性

    #region Public 构造函数

    public IdeographCompletionSet(string moniker, string displayName, ITrackingSpan applicableTo, IEnumerable<Completion> completions, IEnumerable<Completion> completionBuilders)
        : base(moniker, displayName, applicableTo, completions, completionBuilders)
    {
        _innerIdeographCompletionSet = new InnerIdeographCompletionSet(this, new(WritableCompletions), new(WritableCompletionBuilders));
    }

    #endregion Public 构造函数

    #region Public 方法

    public override void Filter()
    {
        _innerIdeographCompletionSet.Filter();
    }

    public override IReadOnlyList<Span> GetHighlightedSpansInDisplayText(string displayText)
    {
        return _innerIdeographCompletionSet.GetHighlightedSpansInDisplayText(displayText);
    }

    public override void SelectBestMatch()
    {
        _innerIdeographCompletionSet.SelectBestMatch();
    }

    #endregion Public 方法
}

internal class IdeographCompletionSet2 : CompletionSet2, IIdeographCompletionSet
{
    #region Private 字段

    private readonly InnerIdeographCompletionSet _innerIdeographCompletionSet;

    #endregion Private 字段

    #region Public 属性

    public override IList<Completion> CompletionBuilders => _innerIdeographCompletionSet.CompletionBuilders;

    public override IList<Completion> Completions => _innerIdeographCompletionSet.Completions;

    #endregion Public 属性

    #region Public 构造函数

    public IdeographCompletionSet2(string moniker, string displayName, ITrackingSpan applicableTo, IEnumerable<Completion> completions, IEnumerable<Completion> completionBuilders, IReadOnlyList<IIntellisenseFilter> filters)
       : base(moniker, displayName, applicableTo, completions, completionBuilders, filters)
    {
        _innerIdeographCompletionSet = new InnerIdeographCompletionSet(this, new(WritableCompletions), new(WritableCompletionBuilders));
    }

    #endregion Public 构造函数

    #region Public 方法

    public override void Filter()
    {
        _innerIdeographCompletionSet.Filter();
    }

    public override IReadOnlyList<Span> GetHighlightedSpansInDisplayText(string displayText)
    {
        return _innerIdeographCompletionSet.GetHighlightedSpansInDisplayText(displayText);
    }

    public override void SelectBestMatch()
    {
        _innerIdeographCompletionSet.SelectBestMatch();
    }

    #endregion Public 方法
}
