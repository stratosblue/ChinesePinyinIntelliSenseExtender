#pragma warning disable IDE0051

using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace System.Text.StringTrie;

internal class UnsafeInfiniteSizeArrayPool<T>
{
    #region Private 字段

    private readonly List<Stack<T[]>> _buckets = new(5);

    #endregion Private 字段

    #region Public 构造函数

    public UnsafeInfiniteSizeArrayPool()
    {
        for (int i = 0; i < 5; i++)
        {
            _buckets.Add(new(8));
        }
    }

    #endregion Public 构造函数

    #region Public 方法

    public T[] Rent(int minimumLength)
    {
        var index = SelectBucketIndex(minimumLength);
        if (index < _buckets.Count)
        {
            if (_buckets[index].Count > 0)
            {
                return _buckets[index].Pop();
            }
            return new T[GetMaxSizeForBucket(index)];
        }
        return new T[minimumLength];
    }

    public void Return(T[] array)
    {
        var index = SelectBucketIndex(array.Length);
        if (index < _buckets.Count)
        {
            _buckets[index].Push(array);
        }
    }

    #endregion Public 方法

    #region Internal 方法

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static int GetMaxSizeForBucket(int binIndex)
    {
        int maxSize = 2 << binIndex;
        return maxSize;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static int SelectBucketIndex(int bufferSize)
    {
#if NETCOREAPP3_0_OR_GREATER
        // https://github.com/dotnet/runtime/blob/release/6.0/src/libraries/System.Private.CoreLib/src/System/Buffers/Utilities.cs#L13
        return BitOperations.Log2((uint)bufferSize - 1 | 1);
#else
        return BitOperationsLog2((uint)bufferSize - 1 | 1);
#endif
    }

    #region compatible

    private static ReadOnlySpan<byte> Log2DeBruijn => new byte[32]
    {
        00, 09, 01, 10, 13, 21, 02, 29,
        11, 14, 16, 18, 22, 25, 03, 30,
        08, 12, 20, 28, 15, 17, 24, 07,
        19, 27, 23, 06, 26, 05, 04, 31
    };

    private static int BitOperationsLog2(uint value)
    {
        // https://github.com/dotnet/runtime/blob/release/6.0/src/libraries/System.Private.CoreLib/src/System/Numerics/BitOperations.cs#L303

        value |= 1;

        // No AggressiveInlining due to large method size
        // Has conventional contract 0->0 (Log(0) is undefined)

        // Fill trailing zeros with ones, eg 00010010 becomes 00011111
        value |= value >> 01;
        value |= value >> 02;
        value |= value >> 04;
        value |= value >> 08;
        value |= value >> 16;

        // uint.MaxValue >> 27 is always in range [0 - 31] so we use Unsafe.AddByteOffset to avoid bounds check
        return Unsafe.AddByteOffset(
            // Using deBruijn sequence, k=2, n=5 (2^5=32) : 0b_0000_0111_1100_0100_1010_1100_1101_1101u
            ref MemoryMarshal.GetReference(Log2DeBruijn),
            // uint|long -> IntPtr cast on 32-bit platforms does expensive overflow checks not needed here
            (IntPtr)(int)((value * 0x07C4ACDDu) >> 27));
    }

    #endregion compatible

    #endregion Internal 方法
}
