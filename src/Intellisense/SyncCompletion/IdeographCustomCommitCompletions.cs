using System.Diagnostics;

using Microsoft.VisualStudio.Language.Intellisense;

namespace ChinesePinyinIntelliSenseExtender.Intellisense.SyncCompletion;

[DebuggerDisplay("{_origin.DisplayText,nq}")]
internal class IdeographCustomCommitCompletion : IdeographCompletion, ICustomCommit, IIdeographCompletion
{
    #region Private 字段

    private readonly ICustomCommit _customCommitter;

    #endregion Private 字段

    #region Public 构造函数

    public IdeographCustomCommitCompletion(string displayText, Completion origin, ICustomCommit customCommitter) : base(displayText, origin)
    {
        _customCommitter = customCommitter;
    }

    #endregion Public 构造函数

    #region Public 方法

    public void Commit()
    {
        _customCommitter.Commit();
    }

    #endregion Public 方法
}

[DebuggerDisplay("{_origin.DisplayText,nq}")]
internal class IdeographCustomCommitCompletion2 : IdeographCompletion2, ICustomCommit, IIdeographCompletion
{
    #region Private 字段

    private readonly ICustomCommit _customCommitter;

    #endregion Private 字段

    #region Public 构造函数

    public IdeographCustomCommitCompletion2(string displayText, Completion2 origin, ICustomCommit customCommitter) : base(displayText, origin)
    {
        _customCommitter = customCommitter;
    }

    #endregion Public 构造函数

    #region Public 方法

    public void Commit()
    {
        _customCommitter.Commit();
    }

    #endregion Public 方法
}

[DebuggerDisplay("{_origin.DisplayText,nq}")]
internal class IdeographCustomCommitCompletion3 : IdeographCompletion3, ICustomCommit, IIdeographCompletion
{
    #region Private 字段

    private readonly ICustomCommit _customCommitter;

    #endregion Private 字段

    #region Public 构造函数

    public IdeographCustomCommitCompletion3(string displayText, Completion3 origin, ICustomCommit customCommitter) : base(displayText, origin)
    {
        _customCommitter = customCommitter;
    }

    #endregion Public 构造函数

    #region Public 方法

    public void Commit()
    {
        _customCommitter.Commit();
    }

    #endregion Public 方法
}

[DebuggerDisplay("{_origin.DisplayText,nq}")]
internal class IdeographCustomCommitCompletion4 : IdeographCompletion4, ICustomCommit, IIdeographCompletion
{
    #region Private 字段

    private readonly ICustomCommit _customCommitter;

    #endregion Private 字段

    #region Public 构造函数

    public IdeographCustomCommitCompletion4(string displayText, string suffix, Completion4 origin, ICustomCommit customCommitter)
        : base(displayText, suffix, origin)
    {
        _customCommitter = customCommitter;
    }

    public IdeographCustomCommitCompletion4(string displayText, Completion4 origin, ICustomCommit customCommitter) : base(displayText, origin)
    {
        _customCommitter = customCommitter;
    }

    #endregion Public 构造函数

    #region Public 方法

    public void Commit()
    {
        _customCommitter.Commit();
    }

    #endregion Public 方法
}

#region Matchable

[DebuggerDisplay("{_origin.DisplayText,nq} [{MatchText,nq}]")]
internal class IdeographMatchableCustomCommitCompletion : IdeographCustomCommitCompletion, ICustomCommit, IIdeographMatchableCompletion
{
    #region Public 属性

    public string MatchText { get; }

    #endregion Public 属性

    #region Public 构造函数

    public IdeographMatchableCustomCommitCompletion(string displayText, string matchText, Completion origin, ICustomCommit customCommitter)
        : base(displayText, origin, customCommitter)
    {
        MatchText = matchText;
    }

    #endregion Public 构造函数
}

[DebuggerDisplay("{_origin.DisplayText,nq} [{MatchText,nq}]")]
internal class IdeographMatchableCustomCommitCompletion2 : IdeographCustomCommitCompletion2, ICustomCommit, IIdeographMatchableCompletion
{
    #region Public 属性

    public string MatchText { get; }

    #endregion Public 属性

    #region Public 构造函数

    public IdeographMatchableCustomCommitCompletion2(string displayText, string matchText, Completion2 origin, ICustomCommit customCommitter)
        : base(displayText, origin, customCommitter)
    {
        MatchText = matchText;
    }

    #endregion Public 构造函数
}

[DebuggerDisplay("{_origin.DisplayText,nq} [{MatchText,nq}]")]
internal class IdeographMatchableCustomCommitCompletion3 : IdeographCustomCommitCompletion3, ICustomCommit, IIdeographMatchableCompletion
{
    #region Public 属性

    public string MatchText { get; }

    #endregion Public 属性

    #region Public 构造函数

    public IdeographMatchableCustomCommitCompletion3(string displayText, string matchText, Completion3 origin, ICustomCommit customCommitter)
        : base(displayText, origin, customCommitter)
    {
        MatchText = matchText;
    }

    #endregion Public 构造函数
}

[DebuggerDisplay("{_origin.DisplayText,nq} [{MatchText,nq}]")]
internal class IdeographMatchableCustomCommitCompletion4 : IdeographCustomCommitCompletion4, ICustomCommit, IIdeographMatchableCompletion
{
    #region Public 属性

    public string MatchText { get; }

    #endregion Public 属性

    #region Public 构造函数

    public IdeographMatchableCustomCommitCompletion4(string displayText, string suffix, string matchText, Completion4 origin, ICustomCommit customCommitter)
        : base(displayText, suffix, origin, customCommitter)
    {
        MatchText = matchText;
    }

    public IdeographMatchableCustomCommitCompletion4(string displayText, string matchText, Completion4 origin, ICustomCommit customCommitter)
        : base(displayText, origin, customCommitter)
    {
        MatchText = matchText;
    }

    #endregion Public 构造函数
}

#endregion Matchable
