using System;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.VisualStudio.Language.Intellisense.AsyncCompletion;
using Microsoft.VisualStudio.Language.Intellisense.AsyncCompletion.Data;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Utilities;

namespace ChinesePinyinIntelliSenseExtender;

[Export(typeof(IAsyncCompletionSourceProvider))]
[Name("中文代码拼音补全")]
[ContentType("text")]
internal class PinyinCompletionSourceProvider : IAsyncCompletionSourceProvider
{
    #region Private 字段

    /// <summary>
    /// 递归标记
    /// </summary>
    private static readonly AsyncLocal<bool> s_recursionTag = new();

    private readonly ConditionalWeakTable<ITextView, IAsyncCompletionSource> _completionSourceCache = new();

    [ImportMany]
    private readonly Lazy<IAsyncCompletionSourceProvider>[] _lazyAsyncCompletionSourceProviders;

    #endregion Private 字段

    #region Public 方法

    public IAsyncCompletionSource GetOrCreate(ITextView textView)
    {
        if (s_recursionTag.Value)
        {
            Debug.WriteLine("Recursion call. Return EmptyAsyncCompletionSource.");
            return EmptyAsyncCompletionSource.Instance;
        }
        s_recursionTag.Value = true;

        try
        {
            if (_lazyAsyncCompletionSourceProviders is null
                || _lazyAsyncCompletionSourceProviders.Length == 0)
            {
                Debug.WriteLine("None AsyncCompletionSourceProvider found. Return EmptyAsyncCompletionSource.");
                return EmptyAsyncCompletionSource.Instance;
            }

            if (_completionSourceCache.TryGetValue(textView, out var itemSource))
            {
                Debug.WriteLine($"CompletionSource cache hit by {textView}.");
                return itemSource;
            }

            Debug.WriteLine($"No completionSource cache for {textView}.");

            var otherAsyncCompletionSources = _lazyAsyncCompletionSourceProviders
                .Select(lazy =>
                {
                    if (lazy.Value is IAsyncCompletionSourceProvider provider
                        && provider.GetType() != typeof(PinyinCompletionSourceProvider))
                    {
                        try
                        {
                            return provider.GetOrCreate(textView);
                        }
                        catch { }
                    }
                    return null;
                })
                .Where(m => m is not null)
                .ToList();

            Debug.WriteLine($"Total {otherAsyncCompletionSources.Count} IAsyncCompletionSource found.");

            IAsyncCompletionSource completionSource = otherAsyncCompletionSources.Count == 0
                                                      ? EmptyAsyncCompletionSource.Instance
                                                      : new PinyinCompletionSource(otherAsyncCompletionSources);

            _completionSourceCache.Add(textView, completionSource);

            return completionSource;
        }
        finally
        {
            s_recursionTag.Value = false;
        }
    }

    #endregion Public 方法

    #region Private 类

    private class EmptyAsyncCompletionSource : IAsyncCompletionSource
    {
        #region Public 属性

        public static EmptyAsyncCompletionSource Instance { get; } = new();

        #endregion Public 属性

        #region Public 方法

        public Task<CompletionContext> GetCompletionContextAsync(IAsyncCompletionSession session, CompletionTrigger trigger, SnapshotPoint triggerLocation, SnapshotSpan applicableToSpan, CancellationToken token) => Task.FromResult<CompletionContext>(null);

        public Task<object> GetDescriptionAsync(IAsyncCompletionSession session, CompletionItem item, CancellationToken token) => Task.FromResult<object>(null);

        public CompletionStartData InitializeCompletion(CompletionTrigger trigger, SnapshotPoint triggerLocation, CancellationToken token) => CompletionStartData.DoesNotParticipateInCompletion;

        #endregion Public 方法
    }

    #endregion Private 类
}
