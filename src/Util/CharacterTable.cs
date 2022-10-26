using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChinesePinyinIntelliSenseExtender.Util;

internal class CharacterTable
{
    public static string tablePath = @"D:\Documents\Codes\source\repos\ChinesePinyinIntelliSenseExtender\src\wubi86_jidian.dict.yaml";
    //private static int skipRows = 23;
    public static char separator = '\t';

    private static Dictionary<(string path, char separator), CharacterTable> singleton = new();
    private Dictionary<string, IEnumerable<string>> cache;

    private CharacterTable() { }

    public static async Task<CharacterTable> MakeTableAsync(string tablePath, char separator)
    {
        if (singleton.TryGetValue((tablePath, separator), out var a)) return a;

        using var f = File.OpenText(tablePath);
        var x = await f.ReadToEndAsync();
        var dict =
            x
                .Split('\n')
                .AsParallel()
                .Where(i => i.Length > 3 && i[1] == separator)
                .Select(i =>
                {
                    var r = i.Split(separator);
                    return (r[0], CapitalizeLeadingCharacter(r[1]));
                }).GroupBy(i => i.Item1)
                .ToDictionary(i => i.Key,
                    i => i.Select(i => i.Item2));

        var r = singleton[(tablePath, separator)] = new CharacterTable
        {
            cache = dict
        };
        return r;
    }

    internal static string CapitalizeLeadingCharacter(string str)
    {
        if (string.IsNullOrEmpty(str)) return string.Empty;
        else if ('a' <= str[0] && str[0] <= 'z')
        {
            return char.ToUpper(str[0]) + str.Substring(1);
        }
        else return str;
    }

    public IEnumerable<string> GetSpells(string str)
    {
        if (cache.ContainsKey(str))
        {
            return cache[str];
        }
        var r = Enumerable.Repeat(str, 1);
        cache[str] = r;
        return r;
    }
}
