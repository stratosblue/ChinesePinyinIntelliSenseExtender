#nullable enable

using System.Buffers;
using System.Collections.Immutable;
using System.Runtime.CompilerServices;

using InputMethodDictionary;

using Microsoft.VisualStudio.Utilities;

namespace ChinesePinyinIntelliSenseExtender.Util;

internal sealed class InputMethodDictionaryGroup : IDisposable
{
    #region Private 字段

    private readonly InputMethodReverseDictionary[] _inputMethodDictionaries;

    private readonly ConditionalWeakTable<string, string[]> _resultCache = new();

    private bool _disposedValue;

    #endregion Private 字段

    #region Public 属性

    public ImmutableArray<InputMethodReverseDictionary> InputMethodDictionaries => _inputMethodDictionaries.ToImmutableArray();

    public bool IsDisposed => _disposedValue;

    #endregion Public 属性

    #region Public 构造函数

    public InputMethodDictionaryGroup(IEnumerable<InputMethodReverseDictionary> inputMethodDictionaries)
    {
        _inputMethodDictionaries = inputMethodDictionaries.Reverse().ToArray();
    }

    #endregion Public 构造函数

    #region Public 方法

    public string[] FindAll(string text)
    {
        if (_disposedValue)
        {
            throw new ObjectDisposedException(nameof(InputMethodDictionaries));
        }

        if (string.IsNullOrEmpty(text))
        {
            return Array.Empty<string>();
        }

        if (_resultCache.TryGetValue(text, out var values))
        {
            return values;
        }

        var pooledStringBuilder = PooledStringBuilder.GetInstance();
        var resultBuffer = ArrayPool<string>.Shared.Rent(32);

        try
        {
            int size = 0;

            var valueSpan = text.AsSpan();
            foreach (var dictionary in _inputMethodDictionaries)
            {
                if (dictionary.TryMatch(valueSpan, out var matchedStringSet))
                {
                    for (int i = 0; i < matchedStringSet.Length; i++)
                    {
                        resultBuffer[size++] = matchedStringSet[i].ToString(pooledStringBuilder.Builder);
                        pooledStringBuilder.Builder.Clear();
                    }
                }
            }

            if (size > 0)
            {
                values = new string[size];
                Array.Copy(resultBuffer, values, size);
            }
            else
            {
                values = Array.Empty<string>();
            }
        }
        finally
        {
            ArrayPool<string>.Shared.Return(resultBuffer);

            pooledStringBuilder.Free();
        }

        _resultCache.TryAdd(text, values);

        return values;
    }

    #endregion Public 方法

    #region IDisposable

    /// <summary>
    ///
    /// </summary>
    ~InputMethodDictionaryGroup()
    {
        Dispose();
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        if (!_disposedValue)
        {
            _disposedValue = true;

            foreach (var item in _inputMethodDictionaries)
            {
                item.Dispose();
            }

            GC.SuppressFinalize(this);
        }
    }

    #endregion IDisposable
}