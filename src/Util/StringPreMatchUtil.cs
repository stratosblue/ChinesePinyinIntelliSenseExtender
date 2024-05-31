using System.Runtime.CompilerServices;
using ChinesePinyinIntelliSenseExtender.Internal;
using ChinesePinyinIntelliSenseExtender.Options;

namespace ChinesePinyinIntelliSenseExtender.Util;

/// <summary>
/// 字符预检查工具
/// </summary>
internal static class StringPreMatchUtil
{
    #region Public 方法

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

    #endregion Public 方法

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

    #region PreCheckPredicates

    internal sealed class FirstCharIsNonAsciiPreCheckPredicate : IPreCheckPredicate
    {
        #region Private 字段

        private static FirstCharIsNonAsciiPreCheckPredicate s_instance;

        #endregion Private 字段

        #region Public 属性

        public static FirstCharIsNonAsciiPreCheckPredicate Instance => s_instance ??= new();

        #endregion Public 属性

        #region Public 方法

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Check(string value) => value.Length > 0 && IsNonAscii(value[0]);

        #endregion Public 方法
    }

    internal sealed class IsNonAsciiPreCheckPredicate : IPreCheckPredicate
    {
        #region Private 字段

        private static IsNonAsciiPreCheckPredicate s_instance;

        #endregion Private 字段

        #region Public 属性

        public static IsNonAsciiPreCheckPredicate Instance => s_instance ??= new();

        #endregion Public 属性

        #region Public 方法

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Check(string value) => IsNonAscii(value);

        #endregion Public 方法
    }

    internal sealed class NoCheckPredicate : IPreCheckPredicate
    {
        #region Private 字段

        private static NoCheckPredicate s_instance;

        #endregion Private 字段

        #region Public 属性

        public static NoCheckPredicate Instance => s_instance ??= new();

        #endregion Public 属性

        #region Public 方法

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Check(string value) => true;

        #endregion Public 方法
    }

    #endregion PreCheckPredicates
}
