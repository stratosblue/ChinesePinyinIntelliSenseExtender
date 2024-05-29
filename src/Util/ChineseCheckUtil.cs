using System.Diagnostics;
using System.Runtime.CompilerServices;
using ChinesePinyinIntelliSenseExtender.Util;

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
            //Debug.WriteLine($"ChineseCheck cache hit by {value}.");
            return isContainsChinese;
        }

        //Debug.WriteLine($"No chineseCheck cache for {value}.");

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

        s_chineseCheckCache.TryAdd(value, isContainsChinese);

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

    internal sealed class ContainsChineseCheckPredicate : IPreCheckPredicate
    {
        private static ContainsChineseCheckPredicate s_instance;
        public static ContainsChineseCheckPredicate Instance => s_instance ??= new();
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Check(string value) => ContainsChinese(value);
    }
    internal sealed class StartWithChineseCheckPredicate : IPreCheckPredicate
    {
        private static StartWithChineseCheckPredicate s_instance;
        public static StartWithChineseCheckPredicate Instance => s_instance ??= new();
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Check(string value) => StartWithChinese(value);
    }

    #endregion Public 方法

    #region Internal 方法

    internal static string CapitalizeLeadingCharacter(this string str)
    {
        if (string.IsNullOrEmpty(str)) return string.Empty;
        else if (char.IsLower(str[0]))
        {
            return char.ToUpper(str[0]) + str.Substring(1);
        }
        else return str;
    }

    #endregion Internal 方法

    #region Private 方法

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool IsChinese(char value) => value >= 0x4e00 && value <= 0x9fd5;

    #endregion Private 方法
}
