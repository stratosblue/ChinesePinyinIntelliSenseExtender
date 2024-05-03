#nullable enable

namespace ChinesePinyinIntelliSenseExtender.Intellisense;

/// <summary>
/// 表意文字完成
/// </summary>
internal interface IIdeographCompletion
{
    #region Public 属性

    /// <summary>
    /// 匹配文本
    /// </summary>
    string? MatchText { get; }

    #endregion Public 属性
}
