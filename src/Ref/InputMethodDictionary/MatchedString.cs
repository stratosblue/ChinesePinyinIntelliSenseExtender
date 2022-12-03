using System.Text;

namespace InputMethodDictionary;

/// <summary>
/// 匹配结果字符串
/// </summary>
public readonly ref struct MatchedString
{
    private readonly UnsafeString[]? _unsafeStrings;

    /// <summary>
    /// <inheritdoc cref="MatchedString"/>
    /// </summary>
    /// <param name="unsafeStrings"></param>
    internal MatchedString(UnsafeString[] unsafeStrings)
    {
        _unsafeStrings = unsafeStrings;
    }

    /// <summary>
    /// 获取匹配后的字符串
    /// </summary>
    /// <returns></returns>
    public override unsafe string ToString()
    {
        if (_unsafeStrings?.Length > 0)
        {
            var stringBuilder = new StringBuilder(16);
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
