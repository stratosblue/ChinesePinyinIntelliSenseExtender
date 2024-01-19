#pragma warning disable CS8500
#pragma warning disable CS8618

using System.Runtime.CompilerServices;

namespace System;

/// <summary>
/// 本机内存储存<para/>
/// 用于优化结构体引用或大量小结构体数组的使用场景，减少内存使用，并提供整体的生命周期管理
/// </summary>
/// <typeparam name="T"></typeparam>
public sealed unsafe class NativeMemoryStore<T>
    : IDisposable
    where T : struct
{
    /// <summary>
    /// 空闲块保留大小（当空闲块数量大于此数量时，将会进行强制淘汰）
    /// </summary>
    private const int FreeBlockRetainedSize = 8;

    /// <summary>
    /// 大内存指针集合的节点大小
    /// </summary>
    private const int LargeMemoryPointerNodeSize = 256;

    /// <summary>
    /// 内存块节点大小
    /// </summary>
    private const int MemoryBlockNodeSize = 128;

    /// <summary>
    /// 可容忍的内存块空闲大小（在申请的空间不能完全填满每个内存块时，该值越大，则分配速度相对可能更快，但可能会浪费更多空间；该值越小，则相反）
    /// </summary>
    private readonly uint _blockIdleTolerationSize;

    #region Large

    /// <summary>
    /// 大内存指针集合
    /// </summary>
    private readonly AppendOnlyCollection<void*[]> _largeMemoryCollection;

    private void*[] _currentLargeMemoryArray;
    private int _currentLargeMemoryArrayIndex = 0;

    #endregion Large

    #region Small

    /// <summary>
    /// 有空间的内存块列表
    /// </summary>
    private readonly AppendOnlyCollectionItem<NativeMemoryBlock<T>>?[] _freeBlocks;

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

    #endregion Small

    private bool _disposedValue;

    /// <summary>
    /// 大内存统计
    /// </summary>
    private ulong _largeMemorySum;

    /// <summary>
    /// 是否已释放
    /// </summary>
    public bool IsDisposed => _disposedValue;

    /// <summary>
    /// <inheritdoc cref="NativeMemoryStore{T}"/>
    /// </summary>
    /// <param name="memoryBlockSize">每个内存块的大小</param>
    /// <param name="memoryBlockNodeSize">内存块存放节点预申请的大小（合理的大小能够节省空间）</param>
    /// <param name="blockIdleTolerationSize">可容忍的内存块空闲大小（在申请的空间不能完全填满每个内存块时，该值越大，则分配速度相对可能更快，但可能会浪费更多空间；该值越小，则相反）</param>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    public NativeMemoryStore(uint memoryBlockSize, uint memoryBlockNodeSize, uint blockIdleTolerationSize)
    {
        if (memoryBlockSize > int.MaxValue)
        {
            throw new ArgumentOutOfRangeException(nameof(memoryBlockSize));
        }

        _blockIdleTolerationSize = blockIdleTolerationSize;

        _memoryBlockSize = memoryBlockSize;

        _memoryBlocks = new AppendOnlyCollection<NativeMemoryBlock<T>>(Convert.ToInt32(memoryBlockNodeSize));
        _freeBlocks = new AppendOnlyCollectionItem<NativeMemoryBlock<T>>?[FreeBlockRetainedSize];
        NewCurrentMemoryBlock();

        _largeMemoryCollection = new(LargeMemoryPointerNodeSize);
        NewCurrentLargeMemoryArray();
    }

    /// <summary>
    /// 计算内存大小（仅计算非托管内存）
    /// </summary>
    /// <returns></returns>
    public ulong CalculateMemorySize() => ((uint)_memoryBlocks.Count() * _memoryBlockSize + _largeMemorySum) * (uint)sizeof(T);

    /// <summary>
    /// 获取指定大小的元素数组的不安全引用
    /// </summary>
    /// <param name="length"></param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public UnsafeMemory<T> Get(int length) => new(GetNaked(length), length);

    /// <summary>
    /// 获取指定大小的元素数组的不安全起始指针
    /// </summary>
    /// <param name="length"></param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public T* GetNaked(int length)
    {
        ThrowIfDisposed();

        return length <= _memoryBlockSize
               ? GetSmallNakedPointer(length)
               : GetLargeNakedPointer(length);
    }

    #region Large

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private T* GetLargeNakedPointer(int length)
    {
        var result = NativeMemoryAllocShim.Alloc<T>(length);
        _largeMemorySum += (uint)length;

        _currentLargeMemoryArray[_currentLargeMemoryArrayIndex++] = result;
        if (_currentLargeMemoryArrayIndex == LargeMemoryPointerNodeSize)
        {
            NewCurrentLargeMemoryArray();
        }
        return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void NewCurrentLargeMemoryArray()
    {
        var newMemoryPointers = new void*[LargeMemoryPointerNodeSize];
        _currentLargeMemoryArray = newMemoryPointers;
        _currentLargeMemoryArrayIndex = 0;
        _largeMemoryCollection.Add(newMemoryPointers);
    }

    #endregion Large

    #region Small

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool FallbackInFreeBlocks(int length, out T* fallbackFoundResult)
    {
        int foundIndex = -1;
        AppendOnlyCollectionItem<NativeMemoryBlock<T>>? localBlock;
        int size;
        for (int i = 0; i < FreeBlockRetainedSize; i++)
        {
            localBlock = _freeBlocks[i];
            if (localBlock == null)
            {
                continue;
            }
            size = localBlock.Value.Item.FreeCount;
            if (size >= length)
            {
                foundIndex = i;
            }
        }
        if (foundIndex >= 0)
        {
            ref var block = ref _freeBlocks[foundIndex]!.Value.Item;
            fallbackFoundResult = block.InternalGet(length);
            if (block.FreeCount < _blockIdleTolerationSize)  //空闲空间冗余可以容忍，不再使用此块
            {
                _freeBlocks[foundIndex] = null;
            }
            return true;
        }
        else
        {
            fallbackFoundResult = null;
            return false;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private T* GetInNewBlock(int length)
    {
        ref var block = ref ResetCurrentMemoryBlock().Item;
        return block.InternalGet(length);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private T* GetSmallNakedPointer(int length)
    {
        ref var block = ref _currentMemoryBlock.Item;
        if (block.FreeCount >= length)
        {
            return block.InternalGet(length);
        }

        return FallbackInFreeBlocks(length, out var fallbackFoundResult)
               ? fallbackFoundResult
               : GetInNewBlock(length);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private AppendOnlyCollectionItem<NativeMemoryBlock<T>> NewCurrentMemoryBlock()
    {
        var newMemoryBlock = new NativeMemoryBlock<T>(_memoryBlockSize);
        return _currentMemoryBlock = _memoryBlocks.Add(newMemoryBlock);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private AppendOnlyCollectionItem<NativeMemoryBlock<T>> ResetCurrentMemoryBlock()
    {
        if (_currentMemoryBlock.Item.FreeCount > _blockIdleTolerationSize)  //空闲大小大于可容忍的大小，则放入空闲块，否则直接丢弃
        {
            int minSizeIndex = -1;
            int minSize = int.MaxValue;
            AppendOnlyCollectionItem<NativeMemoryBlock<T>>? localBlock;
            int size;
            for (int i = 0; i < FreeBlockRetainedSize; i++)
            {
                localBlock = _freeBlocks[i];
                if (localBlock == null)
                {
                    minSizeIndex = i;
                    break;
                }
                size = localBlock.Value.Item.FreeCount;
                if (size < minSize)
                {
                    minSize = size;
                    minSizeIndex = i;
                }
            }
            _freeBlocks[minSizeIndex] = _currentMemoryBlock;
        }

        return NewCurrentMemoryBlock();
    }

    #endregion Small

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
    ~NativeMemoryStore()
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
            foreach (var pointers in _largeMemoryCollection)
            {
                if (pointers is null)
                {
                    continue;
                }
                foreach (var item in pointers)
                {
                    if (item == null)
                    {
                        break;
                    }
                    NativeMemoryAllocShim.Free(item);
                }
            }
            GC.SuppressFinalize(this);
        }
    }

    #endregion IDisposable
}
