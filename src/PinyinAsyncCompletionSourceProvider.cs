#nullable enable

using System;
using System.Collections.Concurrent;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

using ChinesePinyinIntelliSenseExtender.Options;

using Microsoft.VisualStudio.Language.Intellisense.AsyncCompletion;
using Microsoft.VisualStudio.Language.Intellisense.AsyncCompletion.Data;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Utilities;

namespace ChinesePinyinIntelliSenseExtender;

[Export(typeof(IAsyncCompletionSourceProvider))]
[Name("中文代码拼音补全")]
[ContentType("text")]
internal class PinyinAsyncCompletionSourceProvider : IAsyncCompletionSourceProvider
{
    #region Private 字段

    /// <summary>
    /// <see cref="IAsyncCompletionSourceProvider"/> 的类型缓存
    /// </summary>
    private static readonly ConcurrentDictionary<Type, string[]> s_providerContentTypeCache = new();

    /// <summary>
    /// 递归标记
    /// </summary>
    private static readonly AsyncLocal<bool> s_recursionTag = new();

    private readonly ConditionalWeakTable<ITextView, IAsyncCompletionSource> _completionSourceCache = new();

    [ImportMany]
    private readonly Lazy<IAsyncCompletionSourceProvider>[] _lazyAsyncCompletionSourceProviders = null!;

    #endregion Private 字段

    #region Public 构造函数

    static PinyinAsyncCompletionSourceProvider()
    {
        s_providerContentTypeCache.TryAdd(typeof(PinyinAsyncCompletionSourceProvider), Array.Empty<string>());
    }

    #endregion Public 构造函数

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
            if (_completionSourceCache.TryGetValue(textView, out var itemSource))
            {
                Debug.WriteLine($"CompletionSource cache hit by {textView}.");
                return itemSource;
            }
            Debug.WriteLine($"No completionSource cache for {textView}.");

            if (CheckShouldIgnore(textView))
            {
                return EmptyAsyncCompletionSource.Instance;
            }

            var currentContentType = textView.TextBuffer.ContentType;

            var otherAsyncCompletionSources = _lazyAsyncCompletionSourceProviders
                .Where(m => CheckShouldCreateCompletionSource(m.Value, currentContentType))
                .Select(lazy =>
                {
                    try
                    {
                        return lazy.Value.GetOrCreate(textView);
                    }
                    catch { }
                    return null;
                })
                .Where(m => m is not null)
                .ToList()!;

            Debug.WriteLine($"Total {otherAsyncCompletionSources?.Count ?? 0} IAsyncCompletionSource found.");

            IAsyncCompletionSource completionSource = otherAsyncCompletionSources is null || otherAsyncCompletionSources.Count == 0
                                                      ? EmptyAsyncCompletionSource.Instance
                                                      : new PinyinAsyncCompletionSource(otherAsyncCompletionSources!, GeneralOptions.Instance);

            _completionSourceCache.TryAdd(textView, completionSource);

            return completionSource;
        }
        finally
        {
            s_recursionTag.Value = false;
        }
    }

    #endregion Public 方法

    #region Private 方法

    private static bool CheckShouldCreateCompletionSource(IAsyncCompletionSourceProvider? sourceProvider, IContentType contentType)
    {
        if (sourceProvider is null)
        {
            return false;
        }

        var contentTypeValues = s_providerContentTypeCache.GetOrAdd(sourceProvider.GetType(), type =>
        {
            var contentTypeAttributes = type.GetCustomAttributes<ContentTypeAttribute>();
            return contentTypeAttributes.Select(m => m.ContentTypes).ToArray();
        });

        return contentTypeValues.Any(contentType.IsOfType);
    }

    private bool CheckShouldIgnore(ITextView textView)
    {
        if (_lazyAsyncCompletionSourceProviders is null
            || _lazyAsyncCompletionSourceProviders.Length == 0)
        {
            Debug.WriteLine("None AsyncCompletionSourceProvider found. ");
            return true;
        }

        if (!GeneralOptions.Instance.Enable)
        {
            Debug.WriteLine("Extension disabled.");
            return true;
        }

        if (GeneralOptions.Instance.ExcludeExtensionArray.Length > 0
            && textView.TextBuffer.Properties.TryGetProperty<ITextDocument>(typeof(ITextDocument), out var textDocument)
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