namespace InputMethodDictionary;

/// <summary>
/// 文本调整器
/// </summary>
public interface ITextAdjuster
{
    /// <summary>
    /// 处理文本
    /// </summary>
    /// <param name="text">输入文本</param>
    /// <returns>调整后的文本</returns>
    UnsafeMemory<char> Process(UnsafeMemory<char> text);
}