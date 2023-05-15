#nullable enable

using System.Collections.Immutable;

using ChinesePinyinIntelliSenseExtender.Util;

namespace ChinesePinyinIntelliSenseExtender.Options;

/// <summary>
/// 字典组合
/// </summary>
internal class DictionaryCombination : IEquatable<DictionaryCombination>
{
    public string Name { get; set; }

    /// <summary>
    /// 已排序的字典列表
    /// </summary>
    public List<DictionaryDescriptor> OrderedDictionaries { get; set; } = new();

    public DictionaryCombination(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException($"“{nameof(name)}”不能为 null 或空白。", nameof(name));
        }

        Name = name;
    }

    public bool Equals(DictionaryCombination other) => GetHashCode() == other.GetHashCode();

    public override bool Equals(object obj) => obj is DictionaryCombination other && Equals(other);

    public override int GetHashCode()
    {
        int hashCode = -249622208;
        foreach (var descriptor in OrderedDictionaries)
        {
            hashCode = hashCode * -1521134295 + descriptor.GetHashCode();
        }
        return hashCode;
    }
}

internal class DictionaryManageOptions : Options<DictionaryManageOptions>
{
    private List<DictionaryCombination> _dictionaryCombinations = new();

    /// <summary>
    /// 自定义字典
    /// </summary>
    public List<DictionaryDescriptor> CustomDictionaries { get; set; } = new();

    /// <summary>
    /// 使用的字典组合
    /// </summary>
    public List<DictionaryCombination> DictionaryCombinations
    {
        get
        {
            if (_dictionaryCombinations is null || _dictionaryCombinations.Count == 0)
            {
                _dictionaryCombinations = CreateDefaultDictionaryCombinations();
            }
            return _dictionaryCombinations;
        }
        set => _dictionaryCombinations = value;
    }

    private List<DictionaryCombination> CreateDefaultDictionaryCombinations()
    {
        var result = new List<DictionaryCombination>();
        var defaultDictionary = new DictionaryDescriptor(InputMethodDictionaryLoader.PinyinDicPath, "拼音(中文)");
        result.Add(new($"默认 - {defaultDictionary.Name}") { OrderedDictionaries = new() { defaultDictionary } });
        return result;
    }
}

/// <summary>
/// 字典描述符
/// </summary>
internal class DictionaryDescriptor : IEquatable<DictionaryDescriptor>
{
    /// <summary>
    /// 内建的字典列表
    /// </summary>
    public static ImmutableHashSet<DictionaryDescriptor> BuiltInDictionaries { get; } = ImmutableHashSet.Create<DictionaryDescriptor>(new DictionaryDescriptor[]
    {
        new (InputMethodDictionaryLoader.PinyinDicPath,"拼音(中文)"),
        new (InputMethodDictionaryLoader.Wubi86DicPath,"五笔(中文)"),
        new (InputMethodDictionaryLoader.KanaDicPath,"假名(日文)"),
    });

    /// <summary>
    /// 文件路径
    /// </summary>
    public string FilePath { get; set; }

    /// <summary>
    /// 名称
    /// </summary>
    public string Name { get; set; }

    public DictionaryDescriptor(string filePath, string name)
    {
        if (string.IsNullOrWhiteSpace(filePath))
        {
            throw new ArgumentException($"“{nameof(filePath)}”不能为 null 或空白。", nameof(filePath));
        }

        FilePath = filePath;
        Name = name;
    }

    public bool Equals(DictionaryDescriptor other)
    {
        return FilePath == other.FilePath;
    }

    public override int GetHashCode() => FilePath.GetHashCode();

    public override string ToString() => $"{Name}[{FilePath}]";
}
