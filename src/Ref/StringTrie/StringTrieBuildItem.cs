namespace System.Text.StringTrie;

internal readonly struct StringTrieBuildItem<TValue>
    where TValue : struct
{
    #region Public 字段

    public readonly ReadOnlyMemory<char> Current;

    public readonly TValue Value;

    #endregion Public 字段

    #region Public 属性

    public bool IsCompleted => Current.IsEmpty;

    public char Key => Current.Span[0];

    #endregion Public 属性

    #region Public 构造函数

    [Obsolete("must construction with data.", true)]
    public StringTrieBuildItem()
    {
        throw new InvalidOperationException("must construction with data.");
    }

    public StringTrieBuildItem(TValue value, ReadOnlyMemory<char> current)
    {
        Value = value;
        Current = current;
    }

    #endregion Public 构造函数

    #region Public 方法

    public StringTrieBuildItem<TValue> Slice(int start)
    {
        return new(Value, Current.Slice(start));
    }

    /// <inheritdoc/>
    public override string ToString()
    {
        return IsCompleted
               ? $"[End] {Value}"
               : $"[{Key}] {Current} for {Value}";
    }

    #endregion Public 方法
}