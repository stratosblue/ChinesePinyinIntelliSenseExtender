#pragma warning disable CS8500

using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace System;

/// <summary>
/// 本机内存块
/// </summary>
/// <typeparam name="T"></typeparam>
[DebuggerDisplay("{DebuggerDisplay(),nq}")]
public unsafe struct NativeMemoryBlock<T>
    : IDisposable
    where T : struct
{
    private readonly T* _blockPointer;
    private readonly int _capacity;
    private bool _disposed = false;
    private int _freeCount;

    /// <summary>
    /// 容量
    /// </summary>
    public int Capacity => _capacity;

    /// <summary>
    /// 剩余容量
    /// </summary>
    public int FreeCount => _freeCount;

    /// <summary>
    /// 是否已释放
    /// </summary>
    public bool IsDisposed => _disposed;

    /// <summary>
    /// 大小
    /// </summary>
    public int Size => _capacity * sizeof(T);

    /// <summary>
    /// <inheritdoc cref="NativeMemoryBlock{T}"/>
    /// </summary>
    /// <param name="capacity">元素容量</param>
    public NativeMemoryBlock(int capacity)
    {
        _blockPointer = NativeMemoryAllocShim.Alloc<T>(capacity);
        _freeCount = capacity;
        _capacity = capacity;
    }

    /// <summary>
    /// <inheritdoc cref="NativeMemoryBlock{T}"/>
    /// </summary>
    /// <param name="capacity">元素容量</param>
    public NativeMemoryBlock(uint capacity) : this(Convert.ToInt32(capacity))
    {
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        if (!_disposed)
        {
            _disposed = true;
            if (_blockPointer != null)
            {
                NativeMemoryAllocShim.Free(_blockPointer);
            }
        }
    }

    /// <summary>
    /// 尝试从内存块中获取一个指定大小的内存切片
    /// </summary>
    /// <param name="length">需要的内存长度</param>
    /// <param name="span">内存切片</param>
    /// <returns>是否成功获取</returns>
    public bool TryGet(int length, out UnsafeMemory<T> span)
    {
        if (TryGetNaked(length, out var pointer))
        {
            span = new(pointer, length);
            return true;
        }
        span = UnsafeMemory<T>.Empty;
        return false;
    }

    /// <summary>
    /// 尝试从内存块中获取一个指定大小的内存切片指针
    /// </summary>
    /// <param name="length">需要的内存长度</param>
    /// <param name="pointer">内存切片的起始指针</param>
    /// <returns>是否成功获取</returns>
    /// <exception cref="ObjectDisposedException"></exception>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    public bool TryGetNaked(int length, out T* pointer)
    {
        if (_disposed)
        {
            throw new ObjectDisposedException(nameof(NativeMemoryBlock<T>));
        }
        if (length == 0)
        {
            throw new ArgumentOutOfRangeException(nameof(length));
        }
        if (_freeCount >= length)
        {
            pointer = InternalGet(length);

            return true;
        }
        pointer = null;
        return false;
    }

    private string DebuggerDisplay() => _blockPointer is null ? "null" : $"0x{new IntPtr(_blockPointer):x16} [{_freeCount}/{_capacity}]";

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal T* InternalGet(int length)
    {
        var pointer = _blockPointer + (_capacity - _freeCount);
        _freeCount -= length;
        return pointer;
    }
}
