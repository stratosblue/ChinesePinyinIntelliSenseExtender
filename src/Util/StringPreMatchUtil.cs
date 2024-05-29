using System.Runtime.CompilerServices;

using ChinesePinyinIntelliSenseExtender.Options;
using static ChinesePinyinIntelliSenseExtender.ChineseCheckUtil;

namespace ChinesePinyinIntelliSenseExtender.Util;

internal interface IPreCheckPredicate
{
    bool Check(string value);
}

/// <summary>
/// 字符预检查工具
/// </summary>
internal static class StringPreMatchUtil
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IPreCheckPredicate GetPreCheckPredicate(PreMatchType type, StringPreCheckRule rule)
    {
        return type switch
        {
            PreMatchType.FirstChar => rule switch
            {
                StringPreCheckRule.IsChinese => ChineseCheckUtil.StartWithChineseCheckPredicate.Instance,
                _ => FirstCharIsNonAsciiPreCheckPredicate.Instance,
            },
            PreMatchType.FullText => rule switch
            {
                StringPreCheckRule.IsChinese => ChineseCheckUtil.ContainsChineseCheckPredicate.Instance,
                _ => IsNonAsciiPreCheckPredicate.Instance,
            },
            _ => NoCheckPredicate.Instance,
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

    internal sealed class IsNonAsciiPreCheckPredicate : IPreCheckPredicate
    {
        private static IsNonAsciiPreCheckPredicate s_instance;
        public static IsNonAsciiPreCheckPredicate Instance => s_instance ??= new();
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Check(string value) => IsNonAscii(value);
    }
    internal sealed class FirstCharIsNonAsciiPreCheckPredicate : IPreCheckPredicate
    {
        private static FirstCharIsNonAsciiPreCheckPredicate s_instance;
        public static FirstCharIsNonAsciiPreCheckPredicate Instance => s_instance ??= new();
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Check(string value) => value.Length > 0 && IsNonAscii(value[0]);
    }
    internal sealed class NoCheckPredicate : IPreCheckPredicate
    {
        private static NoCheckPredicate s_instance;
        public static NoCheckPredicate Instance => s_instance ??= new();
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Check(string value) => true;
    }

    #endregion NonAsciiCheck
}
