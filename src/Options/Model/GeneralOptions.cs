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

    [Category("基础")]
    [DisplayName("启用")]
    [Description("是否启用拼音联想功能")]
    [DefaultValue(true)]
    public bool Enable { get; set; } = true;

    [Category("基础")]
    [DisplayName("启用多拼写支持")]
    [Description("关闭时，对于在字典里有多种拼写方式的字，只取第一种写法。")]
    [DefaultValue(true)]
    public bool EnableMultipleSpellings { get; set; } = true;

    [Category("基础")]
    [DisplayName("排除的文件拓展名")]
    [Description("多个拓展名使用 \";\" 进行分割，如 \"cs;js\"（修改仅对新开启标签页生效）")]
    [DefaultValue("")]
    public string ExcludeExtensions { get => _excludeExtensions; set => ChangeExcludeExtensions(value); }

    [Category("基础")]
    [DisplayName("使用旧模式")]
    [Description("使用旧的拼写转换模式")]
    [DefaultValue(false)]
    public bool UseLegacy { get; set; } = false;

    #endregion 基础

    #region 字符映射

    [Category("字符映射")]
    [DisplayName("预检查规则")]
    [Description("预检查模式启用时，进行字符检查的规则。IsChinese: 检查是否为中文；NonAscii：检查是否为非Ascii (0x00-0x7F) 字符；")]
    [DefaultValue(StringPreCheckRule.IsChinese)]
    public StringPreCheckRule PreCheckRule { get; set; } = StringPreCheckRule.IsChinese;

    [Category("字符映射")]
    [DisplayName("预检查模式")]
    [Description("在映射前预检查是否需要映射，以提高速度。None: 不检查(所有条目都将尝试进行映射)；FirstChar：仅首字符；FullText：检查所有字符；")]
    [DefaultValue(PreMatchType.FirstChar)]
    public PreMatchType PreMatchType { get; set; } = PreMatchType.FirstChar;

    #endregion 字符映射

    #region 字典

    [Category("字典")]
    [DisplayName("自定附加义字典文件路径")]
    [Description("字典文件每一行的格式：\"汉字 拼写\"（用制表符分隔开）。")]
    [DefaultValue(new string[0])]
    public string[] CustomAdditionalDictionaryPaths { get; set; } = Array.Empty<string>();

    [Category("字典")]
    [DisplayName("启用内置的拼音字典")]
    [Description("启用内置的拼音字典")]
    [DefaultValue(true)]
    public bool EnableBuiltInPinyinDic { get; set; } = true;

    [Category("字典")]
    [DisplayName("启用内置的五笔86字典")]
    [Description("启用内置的五笔86字典")]
    [DefaultValue(false)]
    public bool EnableBuiltInWubi86Dic { get; set; } = false;

    #endregion 字典

    #region 展示

    [Category("展示")]
    [DisplayName("Suffix的展示格式")]
    [Description("参数 {0} 为原 Suffix，参数 {1} 为拼写文本 (置空则展示原文)")]
    [DefaultValue(null)]
    public string? DisplaySuffixFormat { get; set; }

    [Category("展示")]
    [DisplayName("DisplayText的展示格式")]
    [Description("参数 {0} 为原 DisplayText ，参数 {1} 为拼写文本 (置空则展示原文)")]
    [DefaultValue("{0} [{1}]")]
    public string? DisplayTextFormat { get; set; } = "{0} [{1}]";

    [Category("展示")]
    [DisplayName("单个条目展示（旧模式不可用）")]
    [Description("对于多音字/多字典的条目，每个匹配项在完成列表中展示为单独项。")]
    [DefaultValue(true)]
    public bool SingleWordsDisplay { get; set; } = true;

    #endregion 展示

    #region 拓展

    [Category("拓展")]
    [DisplayName("启用F#支持")]
    [Description("启用F#的额外支持")]
    [DefaultValue(true)]
    public bool EnableFSharpSupport { get; set; } = true;

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
