using System.Text.StringTrie;

namespace System;

/// <summary>
/// 针对 <see cref="ReadOnlyMemory{T}"/> 的拓展方法, T 为 <see cref="char"/>
/// </summary>
public static class ReadOnlyMemoryCharExtensions
{
    #region Public 方法

    /// <summary>
    /// 返回去重后的数组
    /// </summary>
    /// <param name="collection"></param>
    /// <returns></returns>
    public static ReadOnlyMemory<char>[] DistinctToArray(this ReadOnlyMemory<char>[] collection) => DistinctToArray(collection.AsMemory());

    /// <summary>
    /// 返回去重后的数组
    /// </summary>
    /// <param name="collection"></param>
    /// <returns></returns>
    public static ReadOnlyMemory<char>[] DistinctToArray(this in ReadOnlyMemory<ReadOnlyMemory<char>> collection)
    {
        return collection.ToArray().Distinct(ReadOnlyMemoryCharEqualityComparer.Instance).ToArray();
    }

    #endregion Public 方法
}
