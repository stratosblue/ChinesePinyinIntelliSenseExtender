namespace System.Runtime.CompilerServices;

internal static class ConditionalWeakTableExtensions
{
    #region Public 方法

    /// <summary>
    /// 添加并忽略可能出现的异常
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    /// <param name="table"></param>
    /// <param name="key"></param>
    /// <param name="value"></param>
    /// <exception cref="NotImplementedException"></exception>
    public static void TryAdd<TKey, TValue>(this ConditionalWeakTable<TKey, TValue> table, TKey key, TValue value)
        where TKey : class
        where TValue : class
    {
        try
        {
            table.Add(key, value);
        }
        catch { }
    }

    #endregion Public 方法
}