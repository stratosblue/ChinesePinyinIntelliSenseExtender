#pragma warning disable CS8500

using System.Runtime.CompilerServices;

namespace System;

/// <summary>
/// 每次分配一个内存大小的本机内存储存<para/>
/// 用于优化结构体引用的使用场景，减少内存使用，并提供整体的生命周期管理
/// </summary>
/// <typeparam name="T"></typeparam>
public sealed unsafe class SingleAllocNativeMemoryStore<T>
    : IDisposable
    where T : struct
{
    /// <summary>
    /// 所有内存块集合
    /// </summary>
    private readonly AppendOnlyCollection<NativeMemoryBlock<T>> _memoryBlocks;

    /// <summary>
    /// 每个内存块的大小
    /// </summary>
    private readonly uint _memoryBlockSize;

    /// <summary>
    /// 当前使用的内存块
    /// </summary>
    private AppendOnlyCollectionItem<NativeMemoryBlock<T>> _currentMemoryBlock;

    private bool _disposedValue;

    /// <summary>
    /// 是否已释放
    /// </summary>
    public bool IsDisposed => _disposedValue;

    /// <summary>
    ///
    /// </summary>
    /// <param name="memoryBlockSize">每个内存块的大小</param>
    /// <param name="memoryBlockNodeSize">内存块存放节点预申请的大小（合理的大小能够节省空间）</param>
    public SingleAllocNativeMemoryStore(uint memoryBlockSize, uint memoryBlockNodeSize)
    {
        if (memoryBlockSize > int.MaxValue)
        {
            throw new ArgumentOutOfRangeException(nameof(memoryBlockSize));
        }
        _memoryBlockSize = memoryBlockSize;

        _memoryBlocks = new AppendOnlyCollection<NativeMemoryBlock<T>>(Convert.ToInt32(memoryBlockNodeSize));

        NewCurrentMemoryBlock();
    }

    /// <summary>
    /// 获取一个元素的引用
    /// </summary>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public T* Get()
    {
        ThrowIfDisposed();
        ref var block = ref _currentMemoryBlock.Item;
        if (block.FreeCount < 1)
        {
            block = ref NewCurrentMemoryBlock().Item;
        }
        return block.InternalGet(1);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private AppendOnlyCollectionItem<NativeMemoryBlock<T>> NewCurrentMemoryBlock()
    {
        var memoryBlock = new NativeMemoryBlock<T>(_memoryBlockSize);
        return _currentMemoryBlock = _memoryBlocks.Add(memoryBlock);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void ThrowIfDisposed()
    {
        if (_disposedValue)
        {
            throw new ObjectDisposedException(nameof(NativeMemoryBlock<T>));
        }
    }

    #region IDisposable

    /// <summary>
    ///
    /// </summary>
    ~SingleAllocNativeMemoryStore()
    {
        Dispose();
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        if (!_disposedValue)
        {
            _disposedValue = true;
            foreach (var block in _memoryBlocks)
            {
                block.Dispose();
            }
            GC.SuppressFinalize(this);
        }
    }

    #endregion IDisposable
}