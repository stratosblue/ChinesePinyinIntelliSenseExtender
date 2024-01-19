using System.Collections;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace System;

/// <summary>
/// 只会增加的集合
/// </summary>
/// <typeparam name="T"></typeparam>
[DebuggerTypeProxy(typeof(AppendOnlyCollectionDebugView<>))]
internal sealed class AppendOnlyCollection<T> : IEnumerable<T>
{
    private readonly int _nodeSize;

    private readonly AppendOnlyCollectionNode<T> _root;

    private AppendOnlyCollectionNode<T> _current;

    /// <summary>
    /// <inheritdoc cref="AppendOnlyCollection{T}"/><para/>
    /// 内部使用多个小节点进行元素存放
    /// </summary>
    /// <param name="nodeSize">每个节点的大小</param>
    public AppendOnlyCollection(int nodeSize)
    {
        var root = new AppendOnlyCollectionNode<T>(nodeSize);
        _nodeSize = nodeSize;
        _root = root;
        _current = root;
    }

    /// <summary>
    /// 添加元素，并获取其引用（当元素为结构体时用于反馈修改）
    /// </summary>
    /// <param name="item"></param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public AppendOnlyCollectionItem<T> Add(T item)
    {
        var result = _current.Add(item);
        if (_current.IsFull)
        {
            _current = _current.NewNext(_nodeSize);
        }
        return result;
    }

    /// <inheritdoc/>
    public IEnumerator<T> GetEnumerator() => new AppendOnlyCollectionNode<T>.AppendOnlyCollectionNodeEnumerator(_root);

    /// <inheritdoc/>
    public override string ToString() => $"Item Count: {this.Count()}";

    /// <inheritdoc/>
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    #region Node

    [DebuggerDisplay("{_index}/{_array.Length}")]
    private sealed class AppendOnlyCollectionNode<TItem>
    {
        private readonly TItem[] _array;
        private int _index;
        private AppendOnlyCollectionNode<TItem>? _next;

        public bool IsFull
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _index >= _array.Length;
        }

        public AppendOnlyCollectionNode(int nodeSize)
        {
            _array = new TItem[nodeSize];
            _index = 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public AppendOnlyCollectionItem<TItem> Add(TItem item)
        {
            _array[_index] = item;
            return new(_array, _index++);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public AppendOnlyCollectionNode<TItem> NewNext(int nodeSize)
        {
            var newNode = new AppendOnlyCollectionNode<TItem>(nodeSize);
            _next = newNode;
            return newNode;
        }

        #region Enumerator

        public sealed class AppendOnlyCollectionNodeEnumerator : IEnumerator<TItem>
        {
            private readonly AppendOnlyCollectionNode<TItem> _root;
            private AppendOnlyCollectionNode<TItem> _current;
            private int _currentIndex = -1;

            public TItem Current => _current._array[_currentIndex];

            object IEnumerator.Current => Current!;

            public AppendOnlyCollectionNodeEnumerator(AppendOnlyCollectionNode<TItem> root)
            {
                _root = root;
                _current = root;
            }

            public void Dispose()
            { }

            public bool MoveNext()
            {
                _currentIndex++;
                if (_currentIndex >= _current._index)
                {
                    if (_current._next is null
                        || _current._next._index < 1)
                    {
                        _currentIndex--;
                        return false;
                    }
                    _current = _current._next;
                    _currentIndex = 0;
                }

                return true;
            }

            public void Reset()
            {
                _current = _root;
                _currentIndex = -1;
            }
        }

        #endregion
    }

    #endregion
}

#region Item

/// <summary>
/// <see cref="AppendOnlyCollection{T}"/> 中某项的引用
/// </summary>
/// <typeparam name="T"></typeparam>
[DebuggerDisplay("{Item}")]
internal readonly struct AppendOnlyCollectionItem<T>
{
    private readonly T[] _array;
    private readonly int _index;

    /// <summary>
    /// 获取该项的引用
    /// </summary>
    public ref T Item => ref _array[_index];

    /// <inheritdoc cref="AppendOnlyCollectionItem{T}"/>
    internal AppendOnlyCollectionItem(T[] array, int index)
    {
        _array = array;
        _index = index;
    }
}

#endregion

#region DebugView

internal sealed class AppendOnlyCollectionDebugView<TItem>
{
    private readonly AppendOnlyCollection<TItem> _collection;

    [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
    public TItem[] Items => _collection.ToArray();

    public AppendOnlyCollectionDebugView(AppendOnlyCollection<TItem> collection)
    {
        _collection = collection;
    }
}

#endregion
