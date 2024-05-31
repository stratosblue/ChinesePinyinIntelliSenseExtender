namespace InputMethodDictionary;

/// <summary>
/// 匹配结果字符串
/// </summary>
public readonly ref struct MatchedString
{
    private readonly UnsafeString[]? _unsafeStrings;

    /// <summary>
    /// 内容长度
    /// </summary>
    public int Length { get; }

    /// <summary>
    /// <inheritdoc cref="MatchedString"/>
    /// </summary>
    /// <param name="unsafeStrings"></param>
    internal MatchedString(UnsafeString[] unsafeStrings)
    {
        _unsafeStrings = unsafeStrings;
        Length = _unsafeStrings.Length;
    }

    /// <summary>
    /// 获取匹配后的字符串
    /// </summary>
    /// <returns></returns>
    public override unsafe string ToString() => ToString(new StringBuilder(16));

    /// <summary>
    /// 获取匹配后的字符串
    /// </summary>
    /// <returns></returns>
    public unsafe string ToString(StringBuilder stringBuilder)
    {
        if (Length > 0)
        {
            foreach (var item in _unsafeStrings)
            {
                fixed (char* ptr = &item.Span.GetPinnableReference())
                {
                    stringBuilder.Append(ptr, item.Span.Length);
                }
            }
            return stringBuilder.ToString();
        }
        return string.Empty;
    }
}
