using System.Windows.Forms;

using ChinesePinyinIntelliSenseExtender.Util;

using Microsoft.VisualStudio.Shell;

namespace ChinesePinyinIntelliSenseExtender.Options;

/// <summary>
/// A provider for custom <see cref="DialogPage" /> implementations.
/// </summary>
internal sealed class OptionPages
{
    #region Public 类

    public sealed class DictionaryManage : OptionPage<DictionaryManageOptions>
    {
        #region Protected 属性

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

        #endregion Protected 属性

        #region Public 方法

        public override void SaveSettingsToStorage()
        {
            base.SaveSettingsToStorage();

            var options = (DictionaryManageOptions)AutomationObject;

            _ = InputMethodDictionaryGroupProvider.LoadFromOptionsAsync(options, default);
        }

        #endregion Public 方法
    }

    public sealed class General : OptionPage<GeneralOptions>
    {
    }

    #endregion Public 类
}
