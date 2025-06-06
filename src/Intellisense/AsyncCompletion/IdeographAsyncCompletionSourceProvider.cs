using System.ComponentModel.Composition;
using System.Diagnostics;

using ChinesePinyinIntelliSenseExtender.Options;

using Microsoft.VisualStudio.Language.Intellisense.AsyncCompletion;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Utilities;

namespace ChinesePinyinIntelliSenseExtender.Intellisense.AsyncCompletion;

[Export(typeof(IAsyncCompletionSourceProvider))]
[Name("表意文字表音补全")]
[ContentType("text")]
internal class IdeographAsyncCompletionSourceProvider : CompletionSourceProviderBase<ITextView, IAsyncCompletionSource>, IAsyncCompletionSourceProvider
{
    #region Private 字段

    [ImportMany]
    private readonly Lazy<IAsyncCompletionSourceProvider>[] _lazyAsyncCompletionSourceProviders = null!;

    #endregion Private 字段

    #region Public 方法

    public IAsyncCompletionSource GetOrCreate(ITextView textView)
    {
        return GetOrCreateCompletionSource(textView);
    }

    #endregion Public 方法

    #region Protected 方法

    protected override IAsyncCompletionSource CreateCompletionSource(ITextView dependence)
    {
        //实验模式时不使用 IdeographAsyncCompletionSource
        if (!Options.Enable
            || Options.AsyncCompletionMode == AsyncCompletionMode.Experimental)
        {
            return null!;
        }

        var currentContentType = dependence.TextBuffer.ContentType;

        var otherAsyncCompletionSources = _lazyAsyncCompletionSourceProviders
            .Where(m => CheckShouldCreateCompletionSource(m.Value, currentContentType))
            .Select(lazy =>
            {
                try
                {
                    return lazy.Value.GetOrCreate(dependence);
                }
                catch { }
                return null;
            })
            .Where(m => m is not null)
            .ToList()!;

        Debug.WriteLine($"Total {otherAsyncCompletionSources?.Count ?? 0} IAsyncCompletionSource found.");

        IAsyncCompletionSource completionSource = otherAsyncCompletionSources is null || otherAsyncCompletionSources.Count == 0
                                                  ? null!
                                                  : new IdeographAsyncCompletionSource(otherAsyncCompletionSources!, Options);

        return completionSource;
    }

    protected override string? GetCurrentEditFilePath(ITextView dependence)
    {
        if (dependence.TextBuffer.Properties.TryGetProperty<ITextDocument>(typeof(ITextDocument), out var textDocument))
        {
            return textDocument?.FilePath;
        }
        return null;
    }

    protected override IAsyncCompletionSource GetDefaultCompletionSource(ITextView dependence)
    {
        return null!;
    }

    #endregion Protected 方法
}
