#pragma warning disable CS8500

namespace System.Text.StringTrie;

/// <summary>
/// Trie 工具
/// </summary>
public class StringTrieUtilities
{
    #region Public 方法

    /// <inheritdoc cref="SplitStringByLine(ReadOnlyMemory{char}, int)"/>
    public static ReadOnlyMemory<char>[] SplitStringByLine(string text, int capacityBase = 16) => SplitStringByLine(text.AsMemory(), capacityBase);

    /// <summary>
    /// 按行拆分 <paramref name="text"/> 为 <see cref="ReadOnlyMemory{T}"/> 数组
    /// </summary>
    /// <param name="text"></param>
    /// <param name="capacityBase">内部 List 的初始化 capacity 基数</param>
    /// <returns></returns>
    public static ReadOnlyMemory<char>[] SplitStringByLine(ReadOnlyMemory<char> text, int capacityBase = 16)
    {
        var textSpan = text.Span;
        var words = new List<ReadOnlyMemory<char>>(text.Length / capacityBase);

        int length = 0;
        int sliceStart = 0;
        for (int index = 0; index < text.Length; index++)
        {
            var current = textSpan[index];
            if (current == '\n'
                && length > 0)
            {
                while (sliceStart < index
                       && char.IsWhiteSpace(textSpan[sliceStart]))
                {
                    length--;
                    sliceStart++;
                }

                if (sliceStart == index)
                {
                    length = 0;
                    sliceStart = index;
                    continue;
                }

                int reverseIndex = sliceStart + length;
                while (reverseIndex > sliceStart
                       && char.IsWhiteSpace(textSpan[reverseIndex]))
                {
                    reverseIndex--;
                }

                if (reverseIndex != sliceStart)
                {
                    words.Add(text.Slice(sliceStart, reverseIndex - sliceStart + 1));
                }

                length = 0;
                sliceStart = index;
            }
            else
            {
                length++;
            }
        }

        return words.ToArray();
    }

    #endregion Public 方法

    #region Internal 方法

    internal static (Dictionary<char, UnsafePooledIncrementalContainer<StringTrieBuildItem<TValue>>> Dictionary, StringTrieBuildItem<TValue>? CompletedItem) CreateChildDictionary<TValue>(UnsafePooledIncrementalContainer<StringTrieBuildItem<TValue>> container)
        where TValue : struct
    {
        StringTrieBuildItem<TValue>? completedItem = null;
        var dictionary = new Dictionary<char, ValueRef<UnsafePooledIncrementalContainer<StringTrieBuildItem<TValue>>>>(container.Length);
        foreach (var item in container.Span)
        {
            if (!item.IsCompleted)
            {
                if (!dictionary.TryGetValue(item.Key, out var subContainerRef))
                {
                    subContainerRef = new(new UnsafePooledIncrementalContainer<StringTrieBuildItem<TValue>>(container.Pool, 2));
                    dictionary[item.Key] = subContainerRef;
                }

                var subContainer = subContainerRef.Value;
                subContainer.Add(item.Slice(1));
                subContainerRef.Value = subContainer;

                continue;
            }

            if (completedItem is not null)
            {
                throw new InvalidDataException($"Endpoint conflict \"{completedItem.Value.Value}\" - \"{item.Value}\".");
            }
            completedItem = item;
        }
        return (dictionary.ToDictionary(m => m.Key, m => m.Value.Value), completedItem);
    }

    internal static unsafe UnsafePooledIncrementalContainer<StringTrieBuildItem<TValue>> CreateContainer<TValue>(UnsafeInfiniteSizeArrayPool<StringTrieBuildItem<TValue>> pool,
                                                                                                                 in ReadOnlyMemory<char>[] strings,
                                                                                                                 in TValue[] values)
        where TValue : struct
    {
        var length = strings.Length;
        var container = new UnsafePooledIncrementalContainer<StringTrieBuildItem<TValue>>(pool, length);
        fixed (TValue* valuesPtr = values)
        fixed (ReadOnlyMemory<char>* stringsPtr = strings)
        {
            for (int i = 0; i < length; i++)
            {
                var item = stringsPtr[i];
                if (!item.Span.IsWhiteSpace())
                {
                    container.Add(new(valuesPtr[i], item));
                }
            }
        }
        return container;
    }

    #endregion Internal 方法
}
