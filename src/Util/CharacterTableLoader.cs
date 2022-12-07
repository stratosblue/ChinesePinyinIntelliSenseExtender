#nullable enable

using System.Diagnostics;
using System.Threading;

namespace ChinesePinyinIntelliSenseExtender.Util;

internal static class CharacterTableLoader
{
    #region Private 字段

    public const char ColumnSeparator = '\t';

    #endregion Private 字段

    #region Public 属性

    public static string PinyinDicPath => Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location), "Assets", "Tables", "pinyin.tsv");

    public static string Wubi86DicPath => Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location), "Assets", "Tables", "wubi86.tsv");

    #endregion Public 属性

    #region Public 方法

    public static Task<CharacterTable> LoadDefaultPinyinAsync(CancellationToken cancellationToken = default)
    {
        return LoadFileAsync(PinyinDicPath, cancellationToken);
    }

    public static Task<CharacterTable> LoadDefaultWubi86Async(CancellationToken cancellationToken = default)
    {
        return LoadFileAsync(Wubi86DicPath, cancellationToken);
    }

    public static async Task<CharacterTable> LoadFileAsync(string path, CancellationToken cancellationToken = default)
    {
        path = path.Trim('\"');
        if (!File.Exists(path))
        {
            throw new FileNotFoundException($"找不到文件 \"{path}\"");
        }

        Debug.WriteLine($"加载字表 '{path}'");
        try
        {
            using var reader = File.OpenText(path);
            Debug.WriteLine($"开始读取字典 '{path}'");
            Stopwatch sw = Stopwatch.StartNew();
            var lines = await reader.ReadToEndAsync();

            cancellationToken.ThrowIfCancellationRequested();

            var dict =
                lines
                    .Split(new[] { '\r', '\n' })
                    .AsParallel()
                    .WithCancellation(cancellationToken)
                    .Where(i => i.Length >= 3 && i[1] == ColumnSeparator)
                    .Select(i =>
                    {
                        var r = i.Split(ColumnSeparator);
                        return (r[0], r[1].CapitalizeLeadingCharacter());
                    })
                    .Where(m => m.Item1.Length == 1)
                    .GroupBy(i => i.Item1[0])
                    .ToDictionary(i => i.Key, i => i.Select(i => i.Item2).Distinct().ToArray());

            sw.Stop();
            Debug.WriteLine($"字典 '{path}' 读取完成, 用时 {sw.Elapsed}");
            return new CharacterTable(dict);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"读取字典 '{path}' 并创建字表时出错: \n{ex}");
            throw;
        }
    }

    #endregion Public 方法
}
