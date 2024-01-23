using System.Collections;

namespace ChinesePinyinIntelliSenseExtender.Internal;

internal sealed class ArrayBaseEnumerator<T> : IEnumerable<T>, IEnumerator<T>
{
    #region Private 字段

    private readonly T[] _array;

    private readonly int _endIndex;

    private readonly int _startIndex;

    private int _index = 0;

    #endregion Private 字段

    #region Public 属性

    public T Current => _array[_index];

    object IEnumerator.Current => _array[_index]!;

    #endregion Public 属性

    #region Public 构造函数

    public ArrayBaseEnumerator(T[] array, int startIndex, int length)
    {
        if (startIndex + length > array.Length)
        {
            throw new ArgumentOutOfRangeException(nameof(array), $"{nameof(startIndex)} + {nameof(length)} > {nameof(array)}.{nameof(array.Length)}");
        }
        _array = array;
        _startIndex = startIndex;

        _endIndex = startIndex + length;

        Reset();
    }

    #endregion Public 构造函数

    #region Public 方法

    public void Dispose()
    {
        Reset();
    }

    public IEnumerator<T> GetEnumerator() => this;

    public bool MoveNext()
    {
        return ++_index < _endIndex;
    }

    public void Reset()
    {
        _index = _startIndex - 1;
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    #endregion Public 方法
}
