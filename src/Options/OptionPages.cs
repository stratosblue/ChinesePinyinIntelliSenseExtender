using System.Windows.Forms;

using ChinesePinyinIntelliSenseExtender.Util;

using Microsoft.VisualStudio.Shell;

namespace ChinesePinyinIntelliSenseExtender.Options;

/// <summary>
/// A provider for custom <see cref="DialogPage" /> implementations.
/// </summary>
internal sealed class OptionPages
{
    public sealed class DictionaryManage : OptionPage<DictionaryManageOptions>
    {
        protected override IWin32Window Window
        {
            get
            {
                var page = new DictionaryManagePage
                {
                    Options = Options
                };
                page.Initialize();
                return page;
            }
        }

        public override void SaveSettingsToStorage()
        {
            base.SaveSettingsToStorage();

            var options = (DictionaryManageOptions)AutomationObject;

            _ = InputMethodDictionaryGroupProvider.LoadFromOptionsAsync(options, default);
        }
    }

    public sealed class General : OptionPage<GeneralOptions>
    {
    }
}