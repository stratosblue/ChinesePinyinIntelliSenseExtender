using System.Runtime.CompilerServices;

using ChinesePinyinIntelliSenseExtender.Options;

namespace ChinesePinyinIntelliSenseExtender.Util;

/// <summary>
/// 字符预检查工具
/// </summary>
internal static class StringPreMatchUtil
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Func<string, bool> GetPreCheckPredicate(PreMatchType type, StringPreCheckRule rule)
    {
        return type switch
        {
            PreMatchType.FirstChar => rule switch
            {
                StringPreCheckRule.IsChinese => ChineseCheckUtil.StartWithChinese,
                _ => static value => value.Length > 0 && IsNonAscii(value[0]),
            },
            PreMatchType.FullText => rule switch
            {
                StringPreCheckRule.IsChinese => ChineseCheckUtil.ContainsChinese,
                _ => IsNonAscii,
            },
            _ => static _ => true,
        };
    }

    #region NonAsciiCheck

    private static readonly ConditionalWeakTable<string, ObjectBoolean> s_nonAsciiCheckCache = new();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsNonAscii(string value)
    {
        if (value is null)
        {
            return false;
        }
        if (s_nonAsciiCheckCache.TryGetValue(value, out var isNonAscii))
        {
            return isNonAscii;
        }

        isNonAscii = ObjectBoolean.False;

        foreach (var item in value)
        {
            if (IsNonAscii(item))
            {
                isNonAscii = ObjectBoolean.True;
                break;
            }
        }

        s_nonAsciiCheckCache.TryAdd(value, isNonAscii);

        return isNonAscii;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsNonAscii(char value) => value > 0x7F;

    #endregion NonAsciiCheck
}
