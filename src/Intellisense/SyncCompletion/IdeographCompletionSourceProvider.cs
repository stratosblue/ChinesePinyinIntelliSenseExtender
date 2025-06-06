using System.ComponentModel.Composition;

using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Utilities;

namespace ChinesePinyinIntelliSenseExtender.Intellisense.SyncCompletion;

[Export(typeof(ICompletionSourceProvider))]
[Name("中文代码拼音补全")]
[ContentType("text")]
internal class IdeographCompletionSourceProvider : CompletionSourceProviderBase<ITextBuffer, ICompletionSource>, ICompletionSourceProvider
{
    #region Public 方法

    public ICompletionSource TryCreateCompletionSource(ITextBuffer textBuffer)
    {
        if (!Options.EnableSyncCompletionSupport)
        {
            return GetDefaultCompletionSource(textBuffer);
        }
        return GetOrCreateCompletionSource(textBuffer);
    }

    #endregion Public 方法

    #region Protected 方法

    protected override ICompletionSource CreateCompletionSource(ITextBuffer dependence)
    {
        var completionSource = new IdeographCompletionSource(Options);
        return completionSource;
    }

    protected override string? GetCurrentEditFilePath(ITextBuffer dependence)
    {
        if (dependence.Properties.TryGetProperty<ITextDocument>(typeof(ITextDocument), out var textDocument))
        {
            return textDocument?.FilePath;
        }
        return null;
    }

    protected override ICompletionSource GetDefaultCompletionSource(ITextBuffer dependence)
    {
        return EmptyCompletionSource.Instance;
    }

    #endregion Protected 方法

    #region Private 类

    private sealed class EmptyCompletionSource : ICompletionSource
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
