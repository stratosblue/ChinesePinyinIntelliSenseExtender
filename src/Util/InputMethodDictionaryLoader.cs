#nullable enable

using System.Diagnostics;
using System.Threading;

using InputMethodDictionary;

namespace ChinesePinyinIntelliSenseExtender.Util;

internal static class InputMethodDictionaryLoader
{
    #region Private 字段

    public const char ColumnSeparator = '\t';

    #endregion Private 字段

    #region Public 属性

    public static string KanaDicPath => Path.Combine(".", "Assets", "Dictionaries", "jap_poly.dict.yaml");

    public static string PinyinDicPath => Path.Combine(".", "Assets", "Dictionaries", "pinyin_simp.dict.yaml");

    public static string Wubi86DicPath => Path.Combine(".", "Assets", "Dictionaries", "wubi86_jidian.dict.yaml");

    #endregion Public 属性

    #region Public 方法

    public static async Task<InputMethodReverseDictionary> LoadFilesAsync(IEnumerable<string> paths, CancellationToken cancellationToken = default)
    {
        var workingDirectory = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);

        var allPath = paths.Select(m => Path.Combine(workingDirectory, m).Trim('\"')).ToArray();

        foreach (var item in allPath)
        {
            if (!File.Exists(item))
            {
                throw new FileNotFoundException($"找不到文件 \"{item}\"");
            }
        }

        Debug.WriteLine($"加载字表 '{string.Join(", ", allPath)}'");
        try
        {
            var texts = new ReadOnlyMemory<char>[allPath.Length];

            Stopwatch sw = Stopwatch.StartNew();

            for (int i = 0; i < allPath.Length; i++)
            {
                var item = allPath[i];

                Debug.WriteLine($"开始读取字典 '{item}'");

                using var reader = File.OpenText(item);
                var sourceFileText = await reader.ReadToEndAsync();
                texts[i] = sourceFileText.AsMemory();
            }

            var inputMethodReverseDictionary = new InputMethodReverseDictionary(InputMethodDictionaryUtilities.CreateGenericReverseMap(texts), TitleCaseTextAdjuster.Instance);

            sw.Stop();
            Debug.WriteLine($"字典 '{string.Join(", ", allPath)}' 读取完成, 用时 {sw.Elapsed}");

            return inputMethodReverseDictionary;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"读取字典 '{string.Join(", ", allPath)}' 并创建字表时出错: \n{ex}");
            throw;
        }
    }

    #endregion Public 方法
}
