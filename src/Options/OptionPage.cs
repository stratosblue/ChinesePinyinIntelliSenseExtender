using Microsoft.VisualStudio.Shell;

namespace ChinesePinyinIntelliSenseExtender.Options;

/// <summary>
/// A base class for a DialogPage to show in Tools -> Options.
/// </summary>
internal class OptionPage<T> : DialogPage where T : Options<T>, new()
{
    protected readonly T Options;

    public override object AutomationObject => Options;

    public OptionPage()
    {
#pragma warning disable VSTHRD102 // Implement internal logic asynchronously
        Options = ThreadHelper.JoinableTaskFactory.Run(Options<T>.CreateAsync);
#pragma warning restore VSTHRD102 // Implement internal logic asynchronously
    }

    public override void LoadSettingsFromStorage()
    {
        Options.Load();
    }

    public override void SaveSettingsToStorage()
    {
        Options.Save();
    }
}
