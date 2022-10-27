using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ChinesePinyinIntelliSenseExtender.Util;

internal class CharacterTable
{
    private const char separator = '\t';
    private static CharacterTable instance;

    private readonly Dictionary<string, IEnumerable<string>> table;
    private readonly ConcurrentDictionary<string, string> cache = new();
    private bool lastDisallowMultipleSpellings;

    public string TablePath { get; }

    private CharacterTable(string tablePath, Dictionary<string, IEnumerable<string>> table)
    {
        this.TablePath = tablePath;
        this.table = table;
    }

    public static async Task<CharacterTable> CreateTableAsync(string tablePath)
    {
        SemaphoreSlim slim = new(1, 1);
        await slim.WaitAsync();

        if (instance != null)
        {
            if (instance.TablePath == tablePath)
            {
                return instance;
            }
        }

        Debug.WriteLine($"字表 '{tablePath}' 不存在，将创建表");
        Dictionary<string, IEnumerable<string>> dict;
        try
        {
            using var reader = File.OpenText(tablePath);
            Debug.WriteLine($"开始读取字典 '{tablePath}'");
            Stopwatch sw = Stopwatch.StartNew();
            var lines = await reader.ReadToEndAsync();

            dict =
                lines
                    .Split('\n')
                    .AsParallel()
                    .Where(i => i.Length > 3 && i[1] == separator)
                    .Select(i =>
                    {
                        var r = i.Split(separator);
                        return (r[0], r[1].CapitalizeLeadingCharacter());
                    }).GroupBy(i => i.Item1)
                    .ToDictionary(i => i.Key, i => i.Select(i => i.Item2));
            sw.Stop();
            Debug.WriteLine($"字典 '{tablePath}' 读取完成, 用时 {sw.Elapsed}");
        }
        catch (Exception e)
        {
            Debug.WriteLine($"读取字典 '{tablePath}' 并创建字表时出错: \n{e}");
            throw;
        }
        finally
        {
            slim.Release();
        }

        return instance = new CharacterTable(tablePath, dict);
    }

    public IEnumerable<string> GetCharacterSpellings(string str)
    {
        if (table.TryGetValue(str, out var r)) return r;
        r = Enumerable.Repeat(str, 1);
        table[str] = r;
        return r;
    }

    public string Convert(string value, bool disallowMultipleSpellings)
    {
        if (disallowMultipleSpellings != lastDisallowMultipleSpellings)
        {
            lastDisallowMultipleSpellings = disallowMultipleSpellings;
            Debug.WriteLine($"DisallowMultipleSpellings changed to {disallowMultipleSpellings}. Cleaning cache.");
            cache.Clear();
        }

        if (cache.TryGetValue(value, out var spellings))
        {
            return spellings;
        }

        Debug.WriteLine($"No spelling caches for {value}.");

        spellings = string.Empty;
        for (int k = 0; k < value.Length; k++)
        {
            var item = value[k];
            var result = GetCharacterSpellings(item.ToString());
            spellings += disallowMultipleSpellings || result.Count() == 1 ? result.First() : ("{" + string.Join("/", result) + "}");
        }
        cache.TryAdd(value, spellings);

        return spellings;
    }
}
