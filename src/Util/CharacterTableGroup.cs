#nullable enable

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

using Microsoft.VisualStudio.Utilities;

namespace ChinesePinyinIntelliSenseExtender.Util;

internal class CharacterTableGroup
{
    #region Private 字段

    private readonly CharacterTable[] _characterTables;

    private readonly ConditionalWeakTable<string, string> _singleSpellingCache = new();
    private readonly ConditionalWeakTable<string, string> _spellingCache = new();

    #endregion Private 字段

    #region Public 属性

    public ImmutableArray<CharacterTable> CharacterTables => _characterTables.ToImmutableArray();

    #endregion Public 属性

    #region Public 构造函数

    public CharacterTableGroup(IEnumerable<CharacterTable> characterTables)
    {
        _characterTables = characterTables.Reverse().ToArray();
    }

    #endregion Public 构造函数

    #region Public 方法

    public string? Convert(string value, bool allowMultipleSpellings)
    {
        if (string.IsNullOrEmpty(value))
        {
            return null;
        }

        var cache = allowMultipleSpellings ? _spellingCache : _singleSpellingCache;

        if (cache.TryGetValue(value, out var spellings))
        {
            return spellings;
        }

        var pooledStringBuilder = PooledStringBuilder.GetInstance();

        try
        {
            spellings = allowMultipleSpellings ? Convert(value, pooledStringBuilder.Builder) : ConvertSingleSpelling(value, pooledStringBuilder.Builder);
        }
        finally
        {
            pooledStringBuilder.Free();
        }

        try
        {
            cache.Add(value, spellings);
        }
        catch { }

        return spellings;

        string Convert(string value, StringBuilder builder)
        {
            foreach (var table in _characterTables)
            {
                builder.Append('[');
                foreach (var item in value)
                {
                    if (table.Query(item) is string itemValue)
                    {
                        builder.Append(itemValue);
                    }
                }
                builder.Append("]/");
            }

            return builder.Length == 0 ? string.Empty : builder.Remove(builder.Length - 1, 1).ToString();
        }

        string ConvertSingleSpelling(string value, StringBuilder builder)
        {
            foreach (var table in _characterTables)
            {
                builder.Append('[');
                foreach (var item in value)
                {
                    if (table.QuerySingle(item) is string itemValue)
                    {
                        builder.Append(itemValue);
                    }
                }
                builder.Append("]/");
            }

            return builder.Length == 0 ? string.Empty : builder.Remove(builder.Length - 1, 1).ToString();
        }
    }

    #endregion Public 方法
}