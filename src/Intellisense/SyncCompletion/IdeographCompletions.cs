using System.Windows.Media;
using Microsoft.VisualStudio.Imaging.Interop;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Utilities;

namespace ChinesePinyinIntelliSenseExtender.Intellisense.SyncCompletion;

internal class IdeographCompletion : Completion, IIdeographCompletion
{
    #region Private 字段

    private readonly Completion _origin;

    #endregion Private 字段

    #region Public 属性

    public override string Description { get => _origin.Description; set => _origin.Description = value; }

    public override string IconAutomationText { get => _origin.IconAutomationText; set => _origin.IconAutomationText = value; }

    public override ImageSource IconSource { get => _origin.IconSource; set => _origin.IconSource = value; }

    public override string InsertionText { get => _origin.InsertionText; set => _origin.InsertionText = value; }

    public override PropertyCollection Properties => _origin.Properties;

    #endregion Public 属性

    #region Public 构造函数

    public IdeographCompletion(string displayText, Completion origin)
    {
        _origin = origin;
        DisplayText = displayText;
    }

    #endregion Public 构造函数
}

internal class IdeographCompletion2 : Completion2, IIdeographCompletion
{
    #region Private 字段

    private readonly Completion2 _origin;

    #endregion Private 字段

    #region Public 属性

    public override IEnumerable<CompletionIcon> AttributeIcons { get => _origin.AttributeIcons; set => _origin.AttributeIcons = value; }

    public override string Description { get => _origin.Description; set => _origin.Description = value; }

    public override string IconAutomationText { get => _origin.IconAutomationText; set => _origin.IconAutomationText = value; }

    public override ImageSource IconSource { get => _origin.IconSource; set => _origin.IconSource = value; }

    public override string InsertionText { get => _origin.InsertionText; set => _origin.InsertionText = value; }

    public override PropertyCollection Properties => _origin.Properties;

    #endregion Public 属性

    #region Public 构造函数

    public IdeographCompletion2(string displayText, Completion2 origin)
    {
        _origin = origin;
        DisplayText = displayText;
    }

    #endregion Public 构造函数
}

internal class IdeographCompletion3 : Completion3, IIdeographCompletion
{
    #region Private 字段

    private readonly Completion3 _origin;

    #endregion Private 字段

    #region Public 属性

    public override IEnumerable<CompletionIcon> AttributeIcons { get => _origin.AttributeIcons; set => _origin.AttributeIcons = value; }

    public override string Description { get => _origin.Description; set => _origin.Description = value; }

    public override string IconAutomationText { get => _origin.IconAutomationText; set => _origin.IconAutomationText = value; }

    public override ImageMoniker IconMoniker => _origin.IconMoniker;

    public override ImageSource IconSource { get => _origin.IconSource; set => _origin.IconSource = value; }

    public override string InsertionText { get => _origin.InsertionText; set => _origin.InsertionText = value; }

    public override PropertyCollection Properties => _origin.Properties;

    #endregion Public 属性

    #region Public 构造函数

    public IdeographCompletion3(string displayText, Completion3 origin)
    {
        _origin = origin;
        DisplayText = displayText;
    }

    #endregion Public 构造函数
}

internal class IdeographCompletion4 : Completion4, IIdeographCompletion
{
    #region Private 字段

    private readonly Completion4 _origin;

    #endregion Private 字段

    #region Public 属性

    public override IEnumerable<CompletionIcon> AttributeIcons { get => _origin.AttributeIcons; set => _origin.AttributeIcons = value; }

    public override string Description { get => _origin.Description; set => _origin.Description = value; }

    public override string IconAutomationText { get => _origin.IconAutomationText; set => _origin.IconAutomationText = value; }

    public override ImageMoniker IconMoniker => _origin.IconMoniker;

    public override ImageSource IconSource { get => _origin.IconSource; set => _origin.IconSource = value; }

    public override string InsertionText { get => _origin.InsertionText; set => _origin.InsertionText = value; }

    public override PropertyCollection Properties => _origin.Properties;

    #endregion Public 属性

    #region Public 构造函数

    public IdeographCompletion4(string displayText, string suffix, Completion4 origin) : base(displayText: null, insertionText: null, description: null, iconMoniker: default, suffix: suffix)
    {
        _origin = origin;
        DisplayText = displayText;
    }

    public IdeographCompletion4(string displayText, Completion4 origin)
    {
        _origin = origin;
        DisplayText = displayText;
    }

    #endregion Public 构造函数
}
