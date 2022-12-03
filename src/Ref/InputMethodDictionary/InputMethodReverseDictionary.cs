using System.Buffers;
using System.Text.StringTrie;

namespace InputMethodDictionary;

/// <summary>
/// 输入法反向匹配字典
/// </summary>
public unsafe class InputMethodReverseDictionary
{
    private readonly StringTrieRoot<UnsafeStringSet> _stringTrieRoot;

    /// <summary>
    /// 目标值存储库
    /// </summary>
    private readonly NativeMemoryStore<char> _targetValueStore;

    /// <summary>
    /// <see cref="UnsafeString"/> 储存库
    /// </summary>
    private readonly NativeMemoryStore<UnsafeString> _unsafeStringStore;

    /// <summary>
    /// <inheritdoc cref="InputMethodReverseDictionary"/>
    /// </summary>
    /// <param name="dictionarySourceText"></param>
    /// <param name="textAdjuster">目标文本调整器</param>
    public InputMethodReverseDictionary(ReadOnlyMemory<char> dictionarySourceText, ITextAdjuster? textAdjuster = null)
        : this(InputMethodDictionaryUtilities.CreateGenericReverseMap(dictionarySourceText), textAdjuster)
    { }

    /// <summary>
    /// <inheritdoc cref="InputMethodReverseDictionary"/>
    /// </summary>
    /// <param name="sourceTargetMap"></param>
    /// <param name="textAdjuster">目标文本调整器</param>
    public InputMethodReverseDictionary(IDictionary<ReadOnlyMemory<char>, List<ReadOnlyMemory<char>>> sourceTargetMap, ITextAdjuster? textAdjuster = null)
    {
        _targetValueStore = new NativeMemoryStore<char>((uint)sourceTargetMap.Count * 4, 8, 32);
        _unsafeStringStore = new((uint)sourceTargetMap.Count / 8, 8, 4);

        var targetValueCache = new Dictionary<ReadOnlyMemory<char>, UnsafeString>(ReadOnlyMemoryCharEqualityComparer.Instance);

        _stringTrieRoot = new StringTrieRoot<UnsafeStringSet>(sourceTargetMap.Keys.ToArray(), key =>
        {
            var targets = sourceTargetMap[key];
            var length = Convert.ToUInt16(targets.Count);
            var targetStrings = _unsafeStringStore.GetNaked(length);

            for (ushort i = 0; i < length; i++)
            {
                var target = targets[i];
                if (targetValueCache.TryGetValue(target, out var targetString))
                {
                    targetStrings[i] = targetString;
                    continue;
                }
                var unsafeMemory = _targetValueStore.Get(target.Length);
                target.Span.CopyTo(unsafeMemory.Span);

                if (textAdjuster is not null)
                {
                    unsafeMemory = textAdjuster.Process(unsafeMemory);
                }

                targetString = new(unsafeMemory);
                targetStrings[i] = targetString;

                targetValueCache.Add(target, targetString);
            }

            return new(targetStrings, length);
        });
    }

    /// <summary>
    /// 匹配 <paramref name="text"/> 中的所有关键字，不能匹配的字符将保留，并返回所有可能的组合
    /// </summary>
    /// <param name="text">要进行匹配的文本</param>
    /// <returns></returns>
    public MatchedStringSet Match(ReadOnlySpan<char> text)
    {
        var all = new List<UnsafeString[]>(text.Length);

        var count = 1;

        while (_stringTrieRoot.TryMatchOne(text, out var matchedStart, out var matchedLength, out var value))
        {
            if (matchedStart > 0)
            {
                var notMatched = text.Slice(0, matchedStart);
                fixed (char* ptr = &notMatched.GetPinnableReference())
                {
                    var unsafeMemory = new UnsafeMemory<char>(ptr, matchedStart);
                    all.Add(new[] { new UnsafeString(unsafeMemory) });
                }
            }

            count *= value.Length;

            var list = new UnsafeString[value.Length];
            for (int i = 0; i < value.Length; i++)
            {
                list[i] = value.Strings[i];
            }
            all.Add(list);
            text = text.Slice(matchedStart + matchedLength);
        }

        if (text.Length > 0)    //还有剩余未匹配
        {
            fixed (char* ptr = &text.GetPinnableReference())
            {
                var unsafeMemory = new UnsafeMemory<char>(ptr, text.Length);
                all.Add(new[] { new UnsafeString(unsafeMemory) });
            }
        }

        var strings = new UnsafeString[count][];
        if (count == 1) //只有一种结果
        {
            strings[0] = all.SelectMany(static m => m).ToArray();
        }
        else    //有多种组合
        {
            var index = 0;

            IndexPermutation(all, indexes =>
            {
                var localValue = new UnsafeString[all.Count];
                for (int i = 0; i < indexes.Length; i++)
                {
                    localValue[i] = all[i][indexes[i]];
                }
                strings[index++] = localValue;
            });
        }

        return new(strings);

        //排列组合索引
        static void IndexPermutation<T>(IEnumerable<IEnumerable<T>> items, Action<int[]> itemPermutationCallback)
        {
            var lengths = items.Select(m => m.Count()).ToArray();

            IndexPermutation(lengths, new int[lengths.Length], itemPermutationCallback, 0);

            static void IndexPermutation(int[] lengths, int[] value, Action<int[]> itemPermutationCallback, int index)
            {
                if (index == lengths.Length)
                {
                    itemPermutationCallback(value);
                    return;
                }

                var current = lengths[index];
                for (int i = 0; i < current; i++)
                {
                    value[index] = i;
                    IndexPermutation(lengths, value, itemPermutationCallback, index + 1);
                }
            }
        }
    }

    /// <summary>
    /// 尝试完整匹配 <paramref name="text"/> 内容
    /// </summary>
    /// <param name="text">要进行匹配的文本</param>
    /// <param name="result">匹配结果</param>
    /// <returns>是否匹配成功</returns>
    public bool TryFullMatch(ReadOnlySpan<char> text, out MatchResult result)
    {
        if (_stringTrieRoot.TryMatch(text, out var unsafeStringSet))
        {
            result = new(unsafeStringSet);
            return true;
        }
        result = default;
        return false;
    }
}
