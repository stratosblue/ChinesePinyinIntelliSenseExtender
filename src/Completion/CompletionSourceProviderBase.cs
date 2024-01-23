#nullable enable

using System.Collections.Concurrent;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;
using ChinesePinyinIntelliSenseExtender.Completion.Async;
using ChinesePinyinIntelliSenseExtender.Options;
using Microsoft.VisualStudio.Utilities;

namespace ChinesePinyinIntelliSenseExtender.Completion;

internal abstract class CompletionSourceProviderBase<TCompletionSourceDependence, TCompletionSource> where TCompletionSourceDependence : class where TCompletionSource : class
{
    #region Protected 字段

    /// <summary>
    /// CompletionSourceProvider 的类型缓存
    /// </summary>
    protected static readonly ConcurrentDictionary<Type, string[]> s_providerContentTypeCache = new();

    /// <summary>
    /// 递归标记
    /// </summary>
    protected static readonly AsyncLocal<bool> s_recursionTag = new();

    #endregion Protected 字段

    #region Private 字段

    private readonly ConditionalWeakTable<TCompletionSourceDependence, TCompletionSource> _completionSourceCache = new();

    #endregion Private 字段

    #region Protected 属性

    protected GeneralOptions Options => GeneralOptions.Instance;

    #endregion Protected 属性

    #region Public 构造函数

    static CompletionSourceProviderBase()
    {
        s_providerContentTypeCache.TryAdd(typeof(PinyinAsyncCompletionSourceProvider), Array.Empty<string>());
    }

    #endregion Public 构造函数

    #region Public 方法

    public virtual TCompletionSource GetOrCreate(TCompletionSourceDependence dependence)
    {
        if (s_recursionTag.Value)
        {
            Debug.WriteLine("Recursion call. Return EmptyAsyncCompletionSource.");
            return GetDefaultCompletionSource(dependence);
        }
        s_recursionTag.Value = true;

        try
        {
            if (_completionSourceCache.TryGetValue(dependence, out var itemSource))
            {
                Debug.WriteLine($"CompletionSource cache hit by {dependence}.");
                return itemSource;
            }
            Debug.WriteLine($"No completionSource cache for {dependence}.");

            if (CheckShouldIgnore(dependence))
            {
                return GetDefaultCompletionSource(dependence);
            }

            var completionSource = CreateCompletionSource(dependence);

            _completionSourceCache.TryAdd(dependence, completionSource);

            return completionSource;
        }
        finally
        {
            s_recursionTag.Value = false;
        }
    }

    #endregion Public 方法

    #region Protected 方法

    protected static bool CheckShouldCreateCompletionSource(object? sourceProvider, IContentType contentType)
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

    protected virtual bool CheckShouldIgnore(TCompletionSourceDependence dependence)
    {
        if (!Options.Enable)
        {
            Debug.WriteLine("Extension disabled.");
            return true;
        }

        if (Options.ExcludeExtensionArray.Length > 0
            && GetCurrentEditFilePath(dependence) is string filePath
            && !string.IsNullOrWhiteSpace(filePath))
        {
            foreach (var item in Options.ExcludeExtensionArray)
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

    protected abstract TCompletionSource CreateCompletionSource(TCompletionSourceDependence dependence);

    protected abstract string? GetCurrentEditFilePath(TCompletionSourceDependence dependence);

    protected abstract TCompletionSource GetDefaultCompletionSource(TCompletionSourceDependence dependence);

    #endregion Protected 方法
}
