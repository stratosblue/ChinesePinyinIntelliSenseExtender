namespace InputMethodDictionary;

/// <summary>
/// 不安全字符串
/// </summary>
internal readonly struct UnsafeString
{
    private readonly UnsafeMemory<char> _value;

    /// <summary>
    /// 对应的切片
    /// </summary>
    public Span<char> Span => _value.Span;

    /// <summary>
    /// <inheritdoc cref="UnsafeString"/>
    /// </summary>
    /// <param name="value"></param>
    public UnsafeString(UnsafeMemory<char> value)
    {
        _value = value;
    }

    /// <inheritdoc/>
    public override string ToString() => _value.Span.ToString();
}
