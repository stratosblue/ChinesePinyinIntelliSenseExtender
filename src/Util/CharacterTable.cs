#nullable enable

using System.Collections.Generic;
using System.Linq;

namespace ChinesePinyinIntelliSenseExtender.Util;

internal class CharacterTable
{
    #region Private 字段

    private readonly Dictionary<char, string> _singleSpellingTable;
    private readonly Dictionary<char, string> _spellingTable;

    #endregion Private 字段

    #region Internal 构造函数

    internal CharacterTable(Dictionary<char, string[]> table)
    {
        var spellingTable = new Dictionary<char, string>(table.Count);
        foreach (var item in table)
        {
            var value = item.Value;
            spellingTable.Add(item.Key, value.Length == 1 ? value[0] : $"{{{string.Join("/", value)}}}");
        }
        _spellingTable = spellingTable;
        _singleSpellingTable = table.ToDictionary(m => m.Key, m => m.Value[0]);
    }

    #endregion Internal 构造函数

    #region Public 方法

    public string? Query(char value)
    {
        if (_spellingTable.TryGetValue(value, out var spelling))
        {
            return spelling;
        }
        return null;
    }

    public string? QuerySingle(char value)
    {
        if (_singleSpellingTable.TryGetValue(value, out var spelling))
        {
            return spelling;
        }

        return null;
    }

    #endregion Public 方法
}
