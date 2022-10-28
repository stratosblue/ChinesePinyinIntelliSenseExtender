using System;
using System.ComponentModel;
using System.Linq;

namespace ChinesePinyinIntelliSenseExtender.Options;

internal class GeneralOptions : Options<GeneralOptions>
{
    private string[]? _excludeExtensionArray;

    private string _excludeExtensions = string.Empty;

    internal string[] ExcludeExtensionArray => _excludeExtensionArray ?? ChangeExcludeExtensions(_excludeExtensions);

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
    [DisplayName("排除的文件拓展名")]
    [Description("多个拓展名使用 \";\" 进行分割，如 \"cs;js\"（修改仅对新开启标签页生效）")]
    [DefaultValue("")]
    public string ExcludeExtensions { get => _excludeExtensions; set => ChangeExcludeExtensions(value); }

    private string[] ChangeExcludeExtensions(string value)
    {
        if (_excludeExtensions == value
            && _excludeExtensionArray is not null)
        {
            return _excludeExtensionArray;
        }
        _excludeExtensionArray = value?.Split(';').Select(m => m.StartsWith(".") ? m : $".{m}").ToArray() ?? Array.Empty<string>();
        _excludeExtensions = value ?? string.Empty;
        return _excludeExtensionArray;
    }

    [Category("基础")]
    [DisplayName("对有多种拼写的字仅取第一种写法")]
    [Description("对于在字典里有多种拼写方式的字，只取第一种写法。")]
    [DefaultValue(false)]
    public bool DisallowMultipleSpellings { get; set; }

    [Category("基础")]
    [DisplayName("字典路径")]
    [Description("字典每一行的格式：\"汉字 拼写\"（用制表符分隔开）。扩展自带了拼音（pinyin.tsv）和五笔86版（wubi86.tsv）的字典，缺省则使用自带的拼音字典。")]
    [DefaultValue("")]
    public string CustomDictionaryPath { get; set; }
}
