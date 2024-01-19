namespace ChinesePinyinIntelliSenseExtender;

internal class ObjectBoolean
{
    #region Public 属性

    public static ObjectBoolean False { get; } = new(false);

    public static ObjectBoolean True { get; } = new(true);

    public bool Value { get; }

    #endregion Public 属性

    #region Private 构造函数

    private ObjectBoolean(bool value)
    {
        Value = value;
    }

    #endregion Private 构造函数

    #region Public 方法

    public static implicit operator bool(ObjectBoolean value) => value.Value;

    public static implicit operator ObjectBoolean(bool value) => value ? True : False;

    #endregion Public 方法
}
