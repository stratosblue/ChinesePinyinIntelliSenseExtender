namespace InputMethodDictionary;

/// <summary>
/// 匹配结果
/// </summary>
public readonly unsafe ref struct MatchResult
{
    private readonly UnsafeStringSet _unsafeStringSet;

    /// <summary>
    /// 长度
    /// </summary>
    public int Length => _unsafeStringSet.Length;

    /// <summary>
    /// 获取结果指定项
    /// </summary>
    /// <param name="index"></param>
    /// <returns></returns>
    public ReadOnlySpan<char> this[int index] => index < Length ? _unsafeStringSet.Strings[index].Span : default;

    /// <summary>
    /// <inheritdoc cref="MatchResult"/>
    /// </summary>
    /// <param name="unsafeStringSet"></param>
    internal MatchResult(UnsafeStringSet unsafeStringSet)
    {
        _unsafeStringSet = unsafeStringSet;
    }

    /// <summary>
    /// 获取结果字符串列表
    /// </summary>
    /// <returns></returns>
    public string[] ToStrings()
    {
        if (Length == 0)
        {
            return Array.Empty<string>();
        }
        var result = new string[Length];

        for (int i = 0; i < _unsafeStringSet.Length; i++)
        {
            result[i] = _unsafeStringSet.Strings[i].ToString();
        }
        return result;
    }
}
