#nullable enable

namespace ChinesePinyinIntelliSenseExtender.Options;

/// <summary>
/// 字符串预检查规则
/// </summary>
public enum StringPreCheckRule
{
    /// <summary>
    /// 是否中文
    /// </summary>
    IsChinese,

    /// <summary>
    /// 非Ascii字符（0x00-0x7F）
    /// </summary>
    NonAscii,

    /// <summary>
    /// 自定义
    /// </summary>
    //Custom
}
