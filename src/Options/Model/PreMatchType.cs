namespace ChinesePinyinIntelliSenseExtender.Options;

/// <summary>
/// 预匹配类型
/// </summary>
public enum PreMatchType
{
    /// <summary>
    /// 不进行预匹配
    /// </summary>
    None,

    /// <summary>
    /// 只匹配第一个字符
    /// </summary>
    FirstChar,

    /// <summary>
    /// 全字符匹配
    /// </summary>
    FullText,
}
