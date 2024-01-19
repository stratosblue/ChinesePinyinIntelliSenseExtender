using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace System.Text.StringTrie;

[DebuggerDisplay("{Length}")]
internal unsafe struct UnsafePooledIncrementalContainer<T> : IDisposable
{
    #region Public 字段

    public readonly UnsafeInfiniteSizeArrayPool<T> Pool;

    #endregion Public 字段

    #region Private 字段

    private T[] _array;
    private int _index;

    #endregion Private 字段

    #region Public 属性

    public int Length => _index;

    public ReadOnlySpan<T> Span => new(_array, 0, _index);

    #endregion Public 属性

    #region Public 构造函数

    [Obsolete("must construction with data.", true)]
    public UnsafePooledIncrementalContainer()
    {
        throw new InvalidOperationException("must construction with data.");
    }

    public UnsafePooledIncrementalContainer(UnsafeInfiniteSizeArrayPool<T> pool, int capacity = 2)
    {
        Pool = pool;
        _array = pool.Rent(capacity);
        _index = 0;
    }

    #endregion Public 构造函数

    #region Public 方法

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Add(in T item)
    {
        if (_index < _array.Length)
        {
            _array[_index++] = item;
        }
        else
        {
            ResizeAdd(item);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Dispose()
    {
        Pool.Return(_array);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public T[] ToArray()
    {
        var result = new T[_index];
        Array.Copy(_array, result, _index);
        return result;
    }

    #endregion Public 方法

    #region Private 方法

    private void ResizeAdd(in T item)
    {
        var newArray = Pool.Rent(_array.Length * 2);
        _array.CopyTo(newArray, 0);
        Pool.Return(_array);
        newArray[_index++] = item;
        _array = newArray;
    }

    #endregion Private 方法
}