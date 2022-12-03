using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace System;

/// <summary>
/// 不安全的内存块<para/>
/// 指针被释放后继续使用会出现非法内存访问
/// </summary>
/// <typeparam name="T"></typeparam>
[DebuggerDisplay("{_pointer} [{_length}]")]
[DebuggerTypeProxy(typeof(UnsafeMemoryView<>))]
public readonly unsafe struct UnsafeMemory<T>
{
    private readonly int _length;
    private readonly void* _pointer;

    /// <summary>
    /// 空内存块
    /// </summary>
    public static UnsafeMemory<T> Empty { get; } = default;

    /// <summary>
    /// 元素长度
    /// </summary>
    public int Length => _length;

    /// <summary>
    /// 获取当前内存块等价的 <see cref="Span{T}"/>
    /// </summary>
    public Span<T> Span => new(_pointer, _length);

    /// <summary>
    /// <inheritdoc cref="UnsafeMemory{T}"/>
    /// </summary>
    /// <param name="pointer">起始指针</param>
    /// <param name="length">元素长度</param>
    public UnsafeMemory(void* pointer, int length)
    {
        _pointer = pointer;
        _length = length;
    }

    /// <summary>
    /// 创建切片
    /// </summary>
    /// <param name="start"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public UnsafeMemory<T> Slice(int start)
    {
        if ((uint)start > (uint)_length)
        {
            throw new ArgumentOutOfRangeException(nameof(start));
        }
        return new(Unsafe.Add<T>(_pointer, start), _length - start);
    }

    /// <summary>
    /// 创建切片
    /// </summary>
    /// <param name="start"></param>
    /// <param name="length"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public UnsafeMemory<T> Slice(int start, int length)
    {
        if ((uint)(start + length) > (uint)_length)
        {
            throw new ArgumentOutOfRangeException(message: $"{nameof(start)} or {nameof(length)} is error.", innerException: null);
        }

        return new(Unsafe.Add<T>(_pointer, start), length);
    }
}

#region DebugView

internal sealed class UnsafeMemoryView<T>
{
    [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
    public T[] Items { get; }

    public UnsafeMemoryView(UnsafeMemory<T> memory)
    {
        Items = memory.Span.ToArray();
    }
}

#endregion