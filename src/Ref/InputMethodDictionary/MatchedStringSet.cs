namespace InputMethodDictionary;

/// <summary>
/// 已匹配的字符串集合
/// </summary>
public readonly unsafe ref struct MatchedStringSet
{
    private readonly UnsafeString[][] _matchedStrings;

    /// <summary>
    /// 长度
    /// </summary>
    public int Length => _matchedStrings?.Length ?? 0;

    /// <summary>
    /// 获取指定的字符串
    /// </summary>
    /// <param name="index"></param>
    /// <returns></returns>
    public MatchedString this[int index] => index < Length ? new(_matchedStrings[index]) : default;

    /// <summary>
    /// <inheritdoc cref="MatchedStringSet"/>
    /// </summary>
    /// <param name="matchedStrings"></param>
    internal MatchedStringSet(UnsafeString[][] matchedStrings)
    {
        _matchedStrings = matchedStrings;
    }

    /// <summary>
    /// 转换为 <see cref="string"/> 列表
    /// </summary>
    /// <returns></returns>
    public string[] ToStrings()
    {
        if (Length == 0)
        {
            return Array.Empty<string>();
        }
        var result = new string[Length];

        for (int i = 0; i < _matchedStrings.Length; i++)
        {
            result[i] = new MatchedString(_matchedStrings[i]).ToString();
        }
        return result;
    }
}
