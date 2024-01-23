#nullable enable

using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Threading;

using ChinesePinyinIntelliSenseExtender.Options;

using Microsoft.VisualStudio.Language.Intellisense.AsyncCompletion;
using Microsoft.VisualStudio.Language.Intellisense.AsyncCompletion.Data;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Utilities;

namespace ChinesePinyinIntelliSenseExtender.Completion.Async;

[Export(typeof(IAsyncCompletionSourceProvider))]
[Name("中文代码拼音补全")]
[ContentType("text")]
internal class PinyinAsyncCompletionSourceProvider : CompletionSourceProviderBase<ITextView, IAsyncCompletionSource>, IAsyncCompletionSourceProvider
{
    #region Private 字段

    [ImportMany]
    private readonly Lazy<IAsyncCompletionSourceProvider>[] _lazyAsyncCompletionSourceProviders = null!;

    #endregion Private 字段

    #region Public 方法

    protected override IAsyncCompletionSource CreateCompletionSource(ITextView dependence)
    {
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
                                                  ? EmptyAsyncCompletionSource.Instance
                                                  : new PinyinAsyncCompletionSource(otherAsyncCompletionSources!, Options);

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
        return EmptyAsyncCompletionSource.Instance;
    }

    #endregion Public 方法

    #region Private 类

    private class EmptyAsyncCompletionSource : IAsyncCompletionSource
    {
        #region Public 属性

        public static EmptyAsyncCompletionSource Instance { get; } = new();

        #endregion Public 属性

        #region Public 方法

        public Task<CompletionContext> GetCompletionContextAsync(IAsyncCompletionSession session, CompletionTrigger trigger, SnapshotPoint triggerLocation, SnapshotSpan applicableToSpan, CancellationToken token) => Task.FromResult<CompletionContext>(null!);

        public Task<object> GetDescriptionAsync(IAsyncCompletionSession session, CompletionItem item, CancellationToken token) => Task.FromResult<object>(null!);

        public CompletionStartData InitializeCompletion(CompletionTrigger trigger, SnapshotPoint triggerLocation, CancellationToken token) => CompletionStartData.DoesNotParticipateInCompletion;

        #endregion Public 方法
    }

    #endregion Private 类
}
