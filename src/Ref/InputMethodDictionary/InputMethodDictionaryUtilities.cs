using System.Text.StringTrie;

namespace InputMethodDictionary;

/// <summary>
/// 输入法字典工具类
/// </summary>
public static class InputMethodDictionaryUtilities
{
    /// <summary>
    /// 数据拆分字符串
    /// </summary>
    public const string DataSplitString = "...";

    /// <summary>
    /// 创建反向匹配字典
    /// </summary>
    /// <param name="dictionarySourceText">字典源字符串</param>
    /// <returns></returns>
    public static Dictionary<ReadOnlyMemory<char>, List<ReadOnlyMemory<char>>> CreateGenericReverseMap(ReadOnlyMemory<char> dictionarySourceText)
    {
        var dataSplitIndex = dictionarySourceText.Span.LastIndexOf(DataSplitString.AsSpan());
        if (dataSplitIndex > 0)
        {
            dictionarySourceText = dictionarySourceText.Slice(dataSplitIndex + DataSplitString.Length);
        }

        var lines = StringTrieUtilities.SplitStringByLine(dictionarySourceText).DistinctToArray();

        var sourceTargetMap = new Dictionary<ReadOnlyMemory<char>, List<ReadOnlyMemory<char>>>(ReadOnlyMemoryCharEqualityComparer.Instance);

        foreach (var line in lines)
        {
            var lineSpan = line.Span;
            var splitIndex1 = lineSpan.IndexOf('\t');
            var splitIndex2 = lineSpan.Slice(splitIndex1 + 1).IndexOf('\t');
            ReadOnlyMemory<char> source = line.Slice(0, splitIndex1);
            ReadOnlyMemory<char> target = splitIndex2 > 0 ? line.Slice(splitIndex1 + 1, splitIndex2) : line.Slice(splitIndex1 + 1);

            // TODO 权重

            if (sourceTargetMap.ContainsKey(source))
            {
                sourceTargetMap[source].Add(target);
            }
            else
            {
                sourceTargetMap.Add(source, new(1) { target });
            }
        }

        return sourceTargetMap;
    }
}
