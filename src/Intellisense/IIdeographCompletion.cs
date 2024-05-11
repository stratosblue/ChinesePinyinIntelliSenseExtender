#nullable enable

namespace ChinesePinyinIntelliSenseExtender.Intellisense;

/// <summary>
/// 表意文字完成项
/// </summary>
internal interface IIdeographCompletion
{
}

/// <summary>
/// 表意文字可匹配完成项
/// </summary>
internal interface IIdeographMatchableCompletion : IIdeographCompletion
{
    #region Public 属性

    /// <summary>
    /// 匹配文本
    /// </summary>
    string MatchText { get; }

    #endregion Public 属性
}
