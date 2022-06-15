using Microsoft.VisualStudio.Shell;

namespace ChinesePinyinIntelliSenseExtender.Options;

/// <summary>
/// A base class for a DialogPage to show in Tools -> Options.
/// </summary>
internal class OptionPage<T> : DialogPage where T : Options<T>, new()
{
    private readonly Options<T> _options;

    public override object AutomationObject => _options;

    public OptionPage()
    {
#pragma warning disable VSTHRD102 // Implement internal logic asynchronously
        _options = ThreadHelper.JoinableTaskFactory.Run(Options<T>.CreateAsync);
#pragma warning restore VSTHRD102 // Implement internal logic asynchronously
    }

    public override void LoadSettingsFromStorage()
    {
        _options.Load();
    }

    public override void SaveSettingsToStorage()
    {
        _options.Save();
    }
}
