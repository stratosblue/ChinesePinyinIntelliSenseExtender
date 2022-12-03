using System.Globalization;

namespace InputMethodDictionary;

/// <summary>
/// 首字母大写文本调整器
/// </summary>
public sealed class TitleCaseTextAdjuster : ITextAdjuster
{
    /// <summary>
    /// TitleCaseTextAdjuster 静态实例
    /// </summary>
    public static TitleCaseTextAdjuster Instance { get; } = new();

    /// <inheritdoc/>
    public UnsafeMemory<char> Process(UnsafeMemory<char> text)
    {
        var trimCount = 0;
        var length = text.Length;
        var targetIndex = 0;
        var lastEmpty = true;

        var span = text.Span;

        char current;
        for (int srcIndex = 0; srcIndex < length; srcIndex++)
        {
            current = span[srcIndex];
            if (lastEmpty)
            {
                span[targetIndex++] = CultureInfo.InvariantCulture.TextInfo.ToUpper(current);
                lastEmpty = false;
                continue;
            }
            if (current == ' ')
            {
                trimCount++;
                lastEmpty = true;
            }
            else
            {
                span[targetIndex++] = current;
            }
        }

        return text.Slice(0, length - trimCount);
    }
}
