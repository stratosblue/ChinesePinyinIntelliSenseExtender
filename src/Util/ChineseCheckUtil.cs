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
                if (IsChinese(item))
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

    public static bool StartWithChinese(string value)
    {
        if (value.Length < 255) //不处理过长的名称
        {
            return IsChinese(value[0]);
        }
        else
        {
            Debug.WriteLine($"ChineseCheck value \"{value}\" is too long. Return false.");
        }

        return false;
    }

    #endregion Public 方法

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool IsChinese(char value) => value >= 0x4e00 && value <= 0x9fd5;

    internal static string CapitalizeLeadingCharacter(this string str)
    {
        if (string.IsNullOrEmpty(str)) return string.Empty;
        else if ('a' <= str[0] && str[0] <= 'z')
        {
            return char.ToUpper(str[0]) + str.Substring(1);
        }
        else return str;
    }
}
