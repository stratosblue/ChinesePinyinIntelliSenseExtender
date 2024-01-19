#pragma warning disable CS8500

using System.Diagnostics;
using System.Runtime.InteropServices;

namespace System.Text.StringTrie;

[DebuggerDisplay("Child Count: {_keyLength} Value: {_value}")]
[StructLayout(LayoutKind.Auto)]
internal readonly unsafe struct StringTrieNode<TValue>
    where TValue : struct
{
    #region Private 字段

    /// <summary>
    /// 子节点 key 列表指针，长度为 <see cref="_keyLength"/>
    /// </summary>
    private readonly ushort* _childKeys;

    /// <summary>
    /// 子节点列表指针，长度为 <see cref="_keyLength"/>
    /// </summary>
    private readonly StringTrieNode<TValue>* _childMaps;

    /// <summary>
    /// 子节点数量
    /// </summary>
    private readonly ushort _keyLength;

    /// <summary>
    /// 当前节点的值
    /// </summary>
    private readonly TValue _value;

    /// <summary>
    /// 是否含有值
    /// </summary>
    private readonly bool _hasValue;

    #endregion Private 字段

    #region Public 属性

    /// <summary>
    /// 当前节点的值
    /// </summary>
    public TValue Value => _value;

    #endregion Public 属性

    #region Public 构造函数

    [Obsolete("must construction with data.", true)]
    public StringTrieNode()
    {
        throw new InvalidOperationException("must construction with data.");
    }

    #endregion Public 构造函数

    #region Internal 构造函数

    internal StringTrieNode(UnsafePooledIncrementalContainer<StringTrieBuildItem<TValue>> container, NativeMemoryStore<ushort> keyStore, NativeMemoryStore<StringTrieNode<TValue>> nodeStore)
    {
        var (dictionary, completedItem) = StringTrieUtilities.CreateChildDictionary(container);

        container.Dispose();

        if (completedItem.HasValue) //为当前节点设置值
        {
            _value = completedItem.Value.Value;
            _hasValue = true;
        }

        // 字典大小不可能大于 char.MaxValue 即 ushort.MaxValue，理论上可以直接强制转换
        _keyLength = (ushort)dictionary.Count;

        if (_keyLength == 0)    //没有子节点
        {
            _childKeys = null;
            _childMaps = null;
            return;
        }

        if (_keyLength == 1)    //只有一个子节点
        {
            var keyValuePair = dictionary.First();   //子节点

            _childKeys = keyStore.GetNaked(1);
            *_childKeys = keyValuePair.Key;
            _childMaps = nodeStore.GetNaked(1);
            *_childMaps = new StringTrieNode<TValue>(keyValuePair.Value, keyStore, nodeStore);
            return;
        }

        //有多个子节点

        var sortedDictionary = new SortedDictionary<char, UnsafePooledIncrementalContainer<StringTrieBuildItem<TValue>>>(dictionary);

        var childKeys = keyStore.GetNaked(_keyLength);
        var childNodes = nodeStore.GetNaked(_keyLength);

        var index = 0;
        foreach (var item in sortedDictionary)
        {
            childKeys[index] = item.Key;
            var value = item.Value;
            if (value.Length > 0)
            {
                childNodes[index] = new StringTrieNode<TValue>(value, keyStore, nodeStore);
            }
            else
            {
                throw new InvalidDataException($"Build fail. Must has items for \"{item.Key}\".");
            }
            index++;
        }

        _childKeys = childKeys;
        _childMaps = childNodes;
    }

    #endregion Internal 构造函数

    #region Public 方法

    public bool TryMatch(ReadOnlySpan<char> text, out int matchedLength, out TValue value)
    {
        fixed (char* textCharPtr = &MemoryMarshal.GetReference(text))
        {
            return InternalMatch(this, (ushort*)textCharPtr, text.Length, out matchedLength, out value);
        }
    }

    #endregion Public 方法

    #region Private 方法

    private static int BinarySearch(ushort* keysPtr, ushort length, ushort value)
    {
        if (length == 1)
        {
            return keysPtr[0] == value ? 0 : -1;
        }

        int lo = 0;
        int hi = length - 1;
        while (lo <= hi)
        {
            int i = (int)((uint)hi + (uint)lo >> 1);

            int c = value - keysPtr[i];
            if (c == 0)
            {
                return i;
            }
            else if (c > 0)
            {
                lo = i + 1;
            }
            else
            {
                hi = i - 1;
            }
        }

        return ~lo;
    }

    private static bool InternalMatch(in StringTrieNode<TValue> node, ushort* text, int length, out int matchedLength, out TValue value)
    {
        int childIndex;
        matchedLength = 0;

        StringTrieNode<TValue> currentNode = node;

        while (matchedLength < length
               && currentNode._childKeys is not null)
        {
            if (currentNode._keyLength == 1)
            {
                childIndex = currentNode._childKeys[0] == *text ? 0 : -1;
            }
            else
            {
                childIndex = BinarySearch(currentNode._childKeys, currentNode._keyLength, *text);
            }

            if (childIndex < 0)
            {
                if (currentNode._hasValue)
                {
                    value = currentNode._value;
                    return true;
                }
                value = default;
                return false;
            }
            currentNode = currentNode._childMaps[childIndex];
            matchedLength++;
            text++;
        }

        value = currentNode._value;
        return currentNode._hasValue;
    }

    #endregion Private 方法
}
