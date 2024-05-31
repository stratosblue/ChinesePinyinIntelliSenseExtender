#nullable enable

using System.ComponentModel;

namespace ChinesePinyinIntelliSenseExtender.Options;

internal class GeneralOptions : Options<GeneralOptions>
{
    #region Private 字段

    private string[]? _excludeExtensionArray;

    private string _excludeExtensions = string.Empty;

    #endregion Private 字段

    #region Public 属性

    #region 基础

    /// <summary>基础/异步完成模式 (Default: 默认模式；Experimental: 实验模式（新模式，使用不同的完成逻辑，响应更快）) </summary>
    [Category("基础")]
    [DisplayName("异步完成模式")]
    [Description("Default: 默认模式；Experimental: 实验模式（新模式，使用不同的完成逻辑，响应更快）")]
    [DefaultValue(AsyncCompletionMode.Default)]
    public AsyncCompletionMode AsyncCompletionMode { get; set; } = AsyncCompletionMode.Default;

    /// <summary>基础/启用 (是否启用拼音联想功能) </summary>
    [Category("基础")]
    [DisplayName("启用")]
    [Description("是否启用拼音联想功能")]
    [DefaultValue(true)]
    public bool Enable { get; set; } = true;

    /// <summary>基础/启用多拼写支持 (关闭时，对于在字典里有多种拼写方式的字，只取第一种写法。) </summary>
    [Category("基础")]
    [DisplayName("启用多拼写支持")]
    [Description("关闭时，对于在字典里有多种拼写方式的字，只取第一种写法。")]
    [DefaultValue(true)]
    public bool EnableMultipleSpellings { get; set; } = true;

    /// <summary>基础/启用同步完成支持（实验） (是否启用对同步完成的实验性支持，如：C++) </summary>
    [Category("基础")]
    [DisplayName("启用同步完成支持（实验）")]
    [Description("是否启用对同步完成的实验性支持，如：C++")]
    [DefaultValue(true)]
    public bool EnableSyncCompletionSupport { get; set; } = true;

    /// <summary>基础/排除的文件拓展名 (多个拓展名使用 \";\" 进行分割，如 \"cs;js\"（修改仅对新开启标签页生效）) </summary>
    [Category("基础")]
    [DisplayName("排除的文件拓展名")]
    [Description("多个拓展名使用 \";\" 进行分割，如 \"cs;js\"（修改仅对新开启标签页生效）")]
    [DefaultValue("")]
    public string ExcludeExtensions { get => _excludeExtensions; set => ChangeExcludeExtensions(value); }

    #endregion 基础

    #region 字符映射

    /// <summary>字符映射/预检查规则 (预检查模式启用时，进行字符检查的规则。IsChinese: 检查是否为中文；NonAscii：检查是否为非Ascii (0x00-0x7F) 字符；) </summary>
    [Category("字符映射")]
    [DisplayName("预检查规则")]
    [Description("预检查模式启用时，进行字符检查的规则。IsChinese: 检查是否为中文；NonAscii：检查是否为非Ascii (0x00-0x7F) 字符；")]
    [DefaultValue(StringPreCheckRule.IsChinese)]
    public StringPreCheckRule PreCheckRule { get; set; } = StringPreCheckRule.IsChinese;

    /// <summary>字符映射/预检查模式 (在映射前预检查是否需要映射，以提高速度。None: 不检查(所有条目都将尝试进行映射)；FirstChar：仅首字符；FullText：检查所有字符；) </summary>
    [Category("字符映射")]
    [DisplayName("预检查模式")]
    [Description("在映射前预检查是否需要映射，以提高速度。None: 不检查(所有条目都将尝试进行映射)；FirstChar：仅首字符；FullText：检查所有字符；")]
    [DefaultValue(PreMatchType.FirstChar)]
    public PreMatchType PreMatchType { get; set; } = PreMatchType.FirstChar;

    #endregion 字符映射

    #region 展示

    /// <summary>展示/Suffix的展示格式 (参数 {0} 为原 Suffix，参数 {1} 为拼写文本 (置空则展示原文)) </summary>
    [Category("展示")]
    [DisplayName("Suffix的展示格式")]
    [Description("参数 {0} 为原 Suffix，参数 {1} 为拼写文本 (置空则展示原文)")]
    [DefaultValue(null)]
    public string? DisplaySuffixFormat { get; set; }

    /// <summary>展示/DisplayText的展示格式 (参数 {0} 为原 DisplayText ，参数 {1} 为拼写文本 (置空则展示原文)) </summary>
    [Category("展示")]
    [DisplayName("DisplayText的展示格式")]
    [Description("参数 {0} 为原 DisplayText ，参数 {1} 为拼写文本 (置空则展示原文)")]
    [DefaultValue("{0} [{1}]")]
    public string? DisplayTextFormat { get; set; } = "{0} [{1}]";

    /// <summary>展示/单个条目展示 (对于多音字/多字典的条目，每个匹配项在完成列表中展示为单独项。) </summary>
    [Category("展示")]
    [DisplayName("单个条目展示")]
    [Description("对于多音字/多字典的条目，每个匹配项在完成列表中展示为单独项。")]
    [DefaultValue(true)]
    public bool SingleWordsDisplay { get; set; } = true;

    /// <summary>展示/同步完成的DisplayText展示格式 (参数 {0} 为拼写文本，参数 {1} 为原 DisplayText (置空则展示拼写文本，务必保证 {0} 在文本头部，否则可能无法匹配)) </summary>
    [Category("展示")]
    [DisplayName("同步完成的DisplayText展示格式")]
    [Description("参数 {0} 为拼写文本，参数 {1} 为原 DisplayText (置空则展示拼写文本，务必保证 {0} 在文本头部，否则可能无法匹配)")]
    [DefaultValue("{0} [{1}]")]
    public string? SyncCompletionDisplayTextFormat { get; set; } = "{0} [{1}]";

    #endregion 展示

    #region 拓展

    /// <summary>拓展/启用F#支持 (启用F#的额外支持) </summary>
    [Category("拓展")]
    [DisplayName("启用F#支持")]
    [Description("启用F#的额外支持")]
    [DefaultValue(false)]
    public bool EnableFSharpSupport { get; set; } = false;

    #endregion 拓展

    #endregion Public 属性

    #region Internal 属性

    internal string[] ExcludeExtensionArray => _excludeExtensionArray ?? ChangeExcludeExtensions(_excludeExtensions);

    #endregion Internal 属性

    #region Private 方法

    private string[] ChangeExcludeExtensions(string value)
    {
        if (_excludeExtensions == value
            && _excludeExtensionArray is not null)
        {
            return _excludeExtensionArray;
        }
        _excludeExtensionArray = value?.Split(';')
                                       .Select(m => m.StartsWith(".") ? m : $".{m}")
                                       .ToArray()
                                 ?? Array.Empty<string>();

        _excludeExtensions = value ?? string.Empty;
        return _excludeExtensionArray;
    }

    #endregion Private 方法
}
