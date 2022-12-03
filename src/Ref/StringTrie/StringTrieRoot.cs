namespace System.Text.StringTrie;

/// <summary>
/// Trie 根<para/>
/// <typeparamref name="TValue"/> 必须为值类型，且不能包含对托管对象的引用或指针（如需引用，请使用手动分配的内存的指针）
/// </summary>
/// <typeparam name="TValue">必须为值类型，且不能包含对托管对象的引用或指针（如需引用，请使用手动分配的内存的指针）</typeparam>
public sealed class StringTrieRoot<TValue>
    : IDisposable
    where TValue : struct
{
    #region Private 字段

    private readonly int[] _fullCharNodeIndexMap;
    private readonly StringTrieNode<TValue>[] _nodes;

    private bool _disposedValue;

    #region Store

    private readonly NativeMemoryStore<ushort> _keyStore;
    private readonly NativeMemoryStore<StringTrieNode<TValue>> _nodeStore;

    #endregion Store

    #endregion Private 字段

    #region Public 属性

    /// <summary>
    /// 是否已释放
    /// </summary>
    public bool IsDisposed => _disposedValue;

    #endregion Public 属性

    #region Public 构造函数

    /// <summary>
    /// <inheritdoc cref="StringTrieRoot{TValue}"/>
    /// </summary>
    /// <param name="strings">字符串列表</param>
    /// <param name="getValueDelegate">获取字符串对应 <typeparamref name="TValue"/> 的委托 </param>
    /// <exception cref="ArgumentException"></exception>
    public StringTrieRoot(in ReadOnlyMemory<ReadOnlyMemory<char>> strings, Func<ReadOnlyMemory<char>, TValue> getValueDelegate) : this(BuildInitializeData(strings, getValueDelegate))
    { }

    /// <summary>
    /// <inheritdoc cref="StringTrieRoot{TValue}"/>
    /// </summary>
    /// <param name="keyValuePairs">键值对</param>
    /// <param name="buildCapacity">指定构建时使用的初始容量</param>
    /// <exception cref="ArgumentException"></exception>
    public StringTrieRoot(IEnumerable<KeyValuePair<ReadOnlyMemory<char>, TValue>> keyValuePairs, int? buildCapacity = null) : this(BuildInitializeData(keyValuePairs, buildCapacity))
    { }

    /// <summary>
    /// <inheritdoc cref="StringTrieRoot{TValue}"/>
    /// </summary>
    /// <param name="strings">字符串列表</param>
    /// <param name="values">字符串对应的值列表</param>
    /// <exception cref="ArgumentException"></exception>
    public StringTrieRoot(in ReadOnlyMemory<ReadOnlyMemory<char>> strings, in ReadOnlyMemory<TValue> values) : this(BuildInitializeData(strings, values))
    { }

    #endregion Public 构造函数

    #region Private 构造函数

    private StringTrieRoot()
    {
        throw new NotImplementedException();
    }

    private StringTrieRoot(InitializeData initializeData)
    {
        var strings = initializeData.Strings;
        var values = initializeData.Values;

        if (strings.Length != values.Length)
        {
            throw new ArgumentException($"\"{nameof(strings)}\" length not match \"{nameof(values)}\".");
        }

        uint memoryBlockSize = (uint)strings.Length / 2;
        uint memoryBlockNodeSize = 8;
        uint blockIdleTolerationSize = 8;

        _keyStore = new(memoryBlockSize, memoryBlockNodeSize, blockIdleTolerationSize);
        _nodeStore = new(memoryBlockSize, memoryBlockNodeSize, blockIdleTolerationSize);

        var buildItemPool = new UnsafeInfiniteSizeArrayPool<StringTrieBuildItem<TValue>>();
        var rootContainer = StringTrieUtilities.CreateContainer(buildItemPool, strings, values);
        var (dictionary, _) = StringTrieUtilities.CreateChildDictionary(rootContainer);

        var fullCharNodeIndexMap = new int[char.MaxValue];

#if NETCOREAPP2_0_OR_GREATER
        Array.Fill(fullCharNodeIndexMap, -1);
#else
        new Span<int>(fullCharNodeIndexMap).Fill(-1);
#endif

        var nodes = new StringTrieNode<TValue>[dictionary.Count];

        var index = 0;
        foreach (var item in dictionary)
        {
            fullCharNodeIndexMap[item.Key] = index;
            nodes[index++] = new StringTrieNode<TValue>(item.Value, _keyStore, _nodeStore);
        }

        _fullCharNodeIndexMap = fullCharNodeIndexMap;
        _nodes = nodes;
    }

    #endregion Private 构造函数

    #region Public 方法

    /// <summary>
    /// 尝试匹配 <paramref name="text"/> 对应的 <typeparamref name="TValue"/>
    /// </summary>
    /// <param name="text">需要进行匹配的字符串</param>
    /// <param name="value">匹配到的 <typeparamref name="TValue"/></param>
    /// <returns>是否成功匹配</returns>
    public bool TryMatch(ReadOnlySpan<char> text, out TValue value)
    {
        if (text.IsEmpty)
        {
            value = default;
            return false;
        }

        var index = _fullCharNodeIndexMap[text[0]];
        if (index < 0)
        {
            value = default;
            return false;
        }

        return _nodes[index].TryMatch(text.Slice(1), out _, out value);
    }

    /// <summary>
    /// 尝试从 <paramref name="text"/> 中匹配一个对应的 <typeparamref name="TValue"/> <para/>
    /// 前缀树对这种需求可能不是最优解。。。
    /// </summary>
    /// <param name="text">需要进行匹配的字符串</param>
    /// <param name="matchedStart">匹配到的源数据开始索引</param>
    /// <param name="matchedLength">匹配到的源数据长度</param>
    /// <param name="value">匹配到的 <typeparamref name="TValue"/></param>
    /// <returns>是否成功匹配</returns>
    public bool TryMatchOne(ReadOnlySpan<char> text, out int matchedStart, out int matchedLength, out TValue value)
    {
        if (text.IsEmpty)
        {
            value = default;
            matchedStart = 0;
            matchedLength = 0;
            return false;
        }

        for (int i = 0; i < text.Length; i++)
        {
            var index = _fullCharNodeIndexMap[text[i]];
            if (index < 0)
            {
                continue;
            }

            if (_nodes[index].TryMatch(text.Slice(i + 1), out var innerMatchedLength, out value))
            {
                matchedStart = i;
                matchedLength = innerMatchedLength + 1;
                return true;
            }
        }

        value = default;
        matchedStart = 0;
        matchedLength = 0;
        return false;
    }

    #endregion Public 方法

    #region IDisposable

    /// <summary>
    ///
    /// </summary>
    ~StringTrieRoot()
    {
        Dispose();
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        if (!_disposedValue)
        {
            _disposedValue = true;
            _keyStore.Dispose();
            _nodeStore.Dispose();
            GC.SuppressFinalize(this);
        }
    }

    #endregion IDisposable

    #region InitializeData

    private static InitializeData BuildCleanInitializeData(in ReadOnlyMemory<ReadOnlyMemory<char>> strings, in ReadOnlyMemory<TValue> values)
    {
        if (strings.Length != values.Length)
        {
            throw new ArgumentException($"\"{nameof(strings)}\" length not match \"{nameof(values)}\".");
        }

        var cleanDictionary = new Dictionary<ReadOnlyMemory<char>, TValue>(strings.Length, ReadOnlyMemoryCharEqualityComparer.Instance);

        var stringsSpan = strings.Span;
        var valuesSpan = values.Span;
        for (int i = 0; i < stringsSpan.Length; i++)
        {
            var key = stringsSpan[i];
            if (key.Span.IsWhiteSpace())
            {
                throw new ArgumentException($"there has empty line at line - {i}.");
            }
            cleanDictionary.Add(key, valuesSpan[i]);
        }

        var stringsArray = new ReadOnlyMemory<char>[cleanDictionary.Count];
        var valuesArray = new TValue[cleanDictionary.Count];

        var index = 0;

        foreach (var item in cleanDictionary)
        {
            stringsArray[index] = item.Key;
            valuesArray[index] = item.Value;
            index++;
        }

        return new(stringsArray, valuesArray);
    }

    private static InitializeData BuildInitializeData(in ReadOnlyMemory<ReadOnlyMemory<char>> strings, Func<ReadOnlyMemory<char>, TValue> getValueDelegate)
    {
        var valuesArray = new TValue[strings.Length];

        var stringsSpan = strings.Span;
        for (int i = 0; i < stringsSpan.Length; i++)
        {
            valuesArray[i] = getValueDelegate(stringsSpan[i]);
        }

        return BuildCleanInitializeData(strings, valuesArray);
    }

    private static InitializeData BuildInitializeData(in ReadOnlyMemory<ReadOnlyMemory<char>> strings, in ReadOnlyMemory<TValue> values) => BuildCleanInitializeData(strings, values);

    private static InitializeData BuildInitializeData(IEnumerable<KeyValuePair<ReadOnlyMemory<char>, TValue>> keyValuePairs, int? buildCapacity = null)
    {
        var stringsContainer = new List<ReadOnlyMemory<char>>(buildCapacity ?? 8);
        var valuesContainer = new List<TValue>(buildCapacity ?? 8);

        foreach (var item in keyValuePairs)
        {
            stringsContainer.Add(item.Key);
            valuesContainer.Add(item.Value);
        }

        return BuildCleanInitializeData(stringsContainer.ToArray(), valuesContainer.ToArray());
    }

    private readonly struct InitializeData
    {
        #region Public 字段

        public readonly ReadOnlyMemory<char>[] Strings;
        public readonly TValue[] Values;

        #endregion Public 字段

        #region Public 构造函数

        public InitializeData(ReadOnlyMemory<char>[] strings, TValue[] values)
        {
            Strings = strings;
            Values = values;
        }

        #endregion Public 构造函数
    }

    #endregion InitializeData
}
