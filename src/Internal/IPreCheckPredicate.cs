namespace ChinesePinyinIntelliSenseExtender.Internal;

internal interface IPreCheckPredicate
{
    #region Public 方法

    bool Check(string value);

    #endregion Public 方法
}
