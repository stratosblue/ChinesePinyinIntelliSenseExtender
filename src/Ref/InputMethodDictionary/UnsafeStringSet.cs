namespace InputMethodDictionary;

/// <summary>
/// 不安全字符串集合
/// </summary>
internal readonly unsafe struct UnsafeStringSet
{
    /// <summary>
    /// 长度
    /// </summary>
    public readonly ushort Length;

    /// <summary>
    /// 字符串指针
    /// </summary>
    public readonly UnsafeString* Strings;

    /// <summary>
    /// <inheritdoc cref="UnsafeStringSet"/>
    /// </summary>
    /// <param name="strings"></param>
    /// <param name="length"></param>
    public UnsafeStringSet(UnsafeString* strings, ushort length)
    {
        Strings = strings;
        Length = length;
    }
}