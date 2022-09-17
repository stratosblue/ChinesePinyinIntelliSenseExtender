using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;

using ChinesePinyinIntelliSenseExtender.Options;

using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Utilities;

namespace ChinesePinyinIntelliSenseExtender;

[Export(typeof(ICompletionSourceProvider))]
[Name("中文代码拼音补全")]
[ContentType("text")]
internal class PinyinCompletionSourceProvider : ICompletionSourceProvider
{
    #region Private 字段

    /// <summary>
    /// 递归标记
    /// </summary>
    private static readonly AsyncLocal<bool> s_recursionTag = new();

    private readonly ConditionalWeakTable<ITextBuffer, ICompletionSource> _completionSourceCache = new();

    #endregion Private 字段

    #region Public 方法

    public ICompletionSource TryCreateCompletionSource(ITextBuffer textBuffer)
    {
        if (s_recursionTag.Value)
        {
            Debug.WriteLine("Recursion call. Return EmptyCompletionSource.");
            return EmptyCompletionSource.Instance;
        }
        s_recursionTag.Value = true;

        try
        {
            if (_completionSourceCache.TryGetValue(textBuffer, out var itemSource))
            {
                Debug.WriteLine($"CompletionSource cache hit by {textBuffer}.");
                return itemSource;
            }
            Debug.WriteLine($"No completionSource cache for {textBuffer}.");

            if (CheckShouldIgnore(textBuffer))
            {
                return EmptyCompletionSource.Instance;
            }

            var completionSource = new PinyinCompletionSource();

            _completionSourceCache.Add(textBuffer, completionSource);

            return completionSource;
        }
        finally
        {
            s_recursionTag.Value = false;
        }
    }

    #endregion Public 方法

    #region Private 方法

    private bool CheckShouldIgnore(ITextBuffer textBuffer)
    {
        if (!GeneralOptions.Instance.Enable)
        {
            Debug.WriteLine("Extension disabled.");
            return true;
        }

        if (GeneralOptions.Instance.ExcludeExtensionArray.Length > 0
            && textBuffer.Properties.TryGetProperty<ITextDocument>(typeof(ITextDocument), out var textDocument)
            && textDocument?.FilePath?.Length > 0)
        {
            var filePath = textDocument.FilePath;
            foreach (var item in GeneralOptions.Instance.ExcludeExtensionArray)
            {
                if (filePath.EndsWith(item))
                {
                    Debug.WriteLine("File extension ignored.");
                    return true;
                }
            }
        }
        return false;
    }

    #endregion Private 方法

    #region Private 类

    private class EmptyCompletionSource : ICompletionSource
    {
        #region Public 属性

        public static EmptyCompletionSource Instance { get; } = new();

        #endregion Public 属性

        #region Public 方法

        public void AugmentCompletionSession(ICompletionSession session, IList<CompletionSet> completionSets)
        {
        }

        public void Dispose()
        {
        }

        #endregion Public 方法
    }

    #endregion Private 类
}
