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

    public static string KanaDicPath => Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location), "Assets", "Dictionaries", "jap_poly.dict.yaml");

    public static string PinyinDicPath => Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location), "Assets", "Dictionaries", "pinyin_simp.dict.yaml");

    public static string Wubi86DicPath => Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location), "Assets", "Dictionaries", "wubi86_jidian.dict.yaml");

    #endregion Public 属性

    #region Public 方法

    public static Task<InputMethodReverseDictionary> LoadDefaultPinyinAsync(CancellationToken cancellationToken = default)
    {
        return LoadFileAsync(PinyinDicPath, cancellationToken);
    }

    public static Task<InputMethodReverseDictionary> LoadDefaultWubi86Async(CancellationToken cancellationToken = default)
    {
        return LoadFileAsync(Wubi86DicPath, cancellationToken);
    }

    public static async Task<InputMethodReverseDictionary> LoadFileAsync(string path, CancellationToken cancellationToken = default)
    {
        path = path.Trim('\"');
        if (!File.Exists(path))
        {
            throw new FileNotFoundException($"找不到文件 \"{path}\"");
        }

        Debug.WriteLine($"加载字表 '{path}'");
        try
        {
            Stopwatch sw = Stopwatch.StartNew();

            Debug.WriteLine($"开始读取字典 '{path}'");

            using var reader = File.OpenText(path);
            var sourceFileText = await reader.ReadToEndAsync();

            var inputMethodReverseDictionary = new InputMethodReverseDictionary(InputMethodDictionaryUtilities.CreateGenericReverseMap(sourceFileText.AsMemory()), TitleCaseTextAdjuster.Instance);

            sw.Stop();
            Debug.WriteLine($"字典 '{path}' 读取完成, 用时 {sw.Elapsed}");

            return inputMethodReverseDictionary;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"读取字典 '{path}' 并创建字表时出错: \n{ex}");
            throw;
        }
    }

    #endregion Public 方法
}
