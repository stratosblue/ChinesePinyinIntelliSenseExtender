#nullable enable

using System;
using System.ComponentModel;
using System.Linq;

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
    [DisplayName("仅判断首字符")]
    [Description("仅检查首字符是否为中文（提高响应速度）")]
    [DefaultValue(false)]
    public bool CheckFirstCharOnly { get; set; } = false;

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

    #endregion 基础

    #region 字典

    [Category("字典")]
    [DisplayName("自定附加义字典路径")]
    [Description("字典每一行的格式：\"汉字 拼写\"（用制表符分隔开）。")]
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
