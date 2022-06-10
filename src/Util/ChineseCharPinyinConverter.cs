using System.Diagnostics;
using System.Runtime.CompilerServices;

using Microsoft.VisualStudio.Utilities;

namespace ChinesePinyinIntelliSenseExtender;

internal static class ChineseCharPinyinConverter
{
    #region Private 字段

    private static readonly ConditionalWeakTable<string, string> s_pinyinCache = new();

    #endregion Private 字段

    #region Public 方法

    public static string Convert(string value)
    {
        if (s_pinyinCache.TryGetValue(value, out var pinyin))
        {
            return pinyin;
        }

        Debug.WriteLine($"No pinyin cache for {value}.");

        pinyin = InternalConvert(value);
        s_pinyinCache.Add(value, pinyin);

        return pinyin;
    }

    #endregion Public 方法

    #region internal

    private static string InternalConvert(string value)
    {
        var pooledStringBuilder = PooledStringBuilder.GetInstance();

        var builder = pooledStringBuilder.Builder;
        try
        {
            foreach (var item in value)
            {
                if (ChineseCharPinyinMap.Map.TryGetValue(item, out var itemValue))
                {
                    builder.Append(itemValue);
                }
                else
                {
                    builder.Append(item);
                }
            }
            return builder.ToString();
        }
        finally
        {
            pooledStringBuilder.Free();
        }
    }

    #endregion internal
}
