namespace System.Text.StringTrie;

/// <summary>
/// <see cref="ReadOnlyMemory{T}"/> T 为 <see cref="char"/> 的 <see cref="IEqualityComparer{T}"/> 实现
/// </summary>
public class ReadOnlyMemoryCharEqualityComparer
    : IEqualityComparer<ReadOnlyMemory<char>>
    , IComparer<ReadOnlyMemory<char>>
{
    /// <summary>
    /// 共享实例
    /// </summary>
    public static ReadOnlyMemoryCharEqualityComparer Instance { get; } = new();

    /// <inheritdoc/>
    public int Compare(ReadOnlyMemory<char> x, ReadOnlyMemory<char> y) => x.Span.CompareTo(y.Span, StringComparison.Ordinal);

    /// <inheritdoc/>
    public bool Equals(ReadOnlyMemory<char> x, ReadOnlyMemory<char> y) => x.Span.SequenceEqual(y.Span);

    /// <inheritdoc/>
    public int GetHashCode(ReadOnlyMemory<char> obj)
    {
#if NETCOREAPP3_0_OR_GREATER
        return string.GetHashCode(obj.Span);
#else
        return GetHashCodeCompatibility(obj);
#endif
    }

    /// <summary>
    /// 获取 HashCode
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    private static unsafe int GetHashCodeCompatibility(ReadOnlyMemory<char> value)
    {
        const int ConvertRate = sizeof(int) / sizeof(char);

        fixed (char* charPointer = &value.Span.GetPinnableReference())
        {
            int* int32Pointer = (int*)charPointer;

            var length = value.Length / ConvertRate;
            unchecked
            {
                var result = 0;

                for (int i = 0; i < length; i++)
                {
                    result = (result * 31) ^ *(int32Pointer++);
                }

                var remainder = value.Length % ConvertRate;
                if (remainder > 0)
                {
                    short* int16Pointer = (short*)int32Pointer;
                    for (int i = 0; i < remainder; i++)
                    {
                        result = (result * 31) ^ *(int16Pointer++);
                    }
                }

                return result;
            }
        }
    }
}