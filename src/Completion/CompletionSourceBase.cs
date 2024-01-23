#nullable enable

using System.Threading;

using ChinesePinyinIntelliSenseExtender.Options;
using ChinesePinyinIntelliSenseExtender.Util;
using Microsoft.VisualStudio.Utilities;

namespace ChinesePinyinIntelliSenseExtender.Completion;

internal abstract class CompletionSourceBase
{
    #region Protected 字段

    /// <summary>
    /// 递归标记
    /// </summary>
    protected static readonly AsyncLocal<bool> s_completionContextRecursionTag = new();

    protected readonly GeneralOptions Options;

    #endregion Protected 字段

    #region Private 字段

    private InputMethodDictionaryGroup? _inputMethodDictionaryGroup;

    #endregion Private 字段

    #region Public 构造函数

    public CompletionSourceBase(GeneralOptions options)
    {
        Options = options ?? throw new ArgumentNullException(nameof(options));
    }

    #endregion Public 构造函数

    #region Protected 方法

    protected static string FormatString(string? format, string origin, string spellings)
    {
        if (string.IsNullOrEmpty(format))
        {
            return origin;
        }
        var builder = PooledStringBuilder.GetInstance();
        try
        {
            return builder.Builder.AppendFormat(format, origin, spellings).ToString();
        }
        finally
        {
            builder.Free();
        }
    }

    protected bool CanNotProcess()
    {
        return s_completionContextRecursionTag.Value
               || !Options.Enable;
    }

    protected InputMethodDictionaryGroup GetInputMethodDictionaryGroup()
    {
        if (_inputMethodDictionaryGroup is null
            || _inputMethodDictionaryGroup.IsDisposed)
        {
            _inputMethodDictionaryGroup = InputMethodDictionaryGroupProvider.GetAsync().ConfigureAwait(false).GetAwaiter().GetResult();
        }
        return _inputMethodDictionaryGroup;
    }

    protected async Task<InputMethodDictionaryGroup> GetInputMethodDictionaryGroupAsync()
    {
        if (_inputMethodDictionaryGroup is null
            || _inputMethodDictionaryGroup.IsDisposed)
        {
            _inputMethodDictionaryGroup = await InputMethodDictionaryGroupProvider.GetAsync();
        }
        return _inputMethodDictionaryGroup;
    }

    #endregion Protected 方法
}
