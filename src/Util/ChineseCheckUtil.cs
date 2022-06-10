using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace ChinesePinyinIntelliSenseExtender;

internal static class ChineseCheckUtil
{
    #region Private 字段

    private static readonly ConditionalWeakTable<string, ObjectBoolean> s_chineseCheckCache = new();

    #endregion Private 字段

    #region Public 方法

    public static bool ContainsChinese(string value)
    {
        if (s_chineseCheckCache.TryGetValue(value, out var isContainsChinese))
        {
            Debug.WriteLine($"ChineseCheck cache hit by {value}.");
            return isContainsChinese;
        }

        Debug.WriteLine($"No chineseCheck cache for {value}.");

        isContainsChinese = ObjectBoolean.False;

        if (value.Length < 255) //不处理过长的名称
        {
            foreach (var item in value)
            {
                if (item >= 0x4e00 && item <= 0x9fbb)
                {
                    isContainsChinese = ObjectBoolean.True;
                    break;
                }
            }
        }
        else
        {
            Debug.WriteLine($"ChineseCheck value \"{value}\" is too long. Return false.");
        }

        s_chineseCheckCache.Add(value, isContainsChinese);

        return isContainsChinese;
    }

    #endregion Public 方法
}
