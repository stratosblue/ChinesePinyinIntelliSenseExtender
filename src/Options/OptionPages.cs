using ChinesePinyinIntelliSenseExtender.Util;

namespace ChinesePinyinIntelliSenseExtender.Options;

/// <summary>
/// A provider for custom <see cref="DialogPage" /> implementations.
/// </summary>
internal sealed class OptionPages
{
    public sealed class General : OptionPage<GeneralOptions>
    {
        public override void SaveSettingsToStorage()
        {
            base.SaveSettingsToStorage();

            var options = (GeneralOptions)AutomationObject;

            if (options.UseLegacy)
            {
                _ = CharacterTableGroupProvider.LoadFromOptionsAsync(options, default);
            }
            else
            {
                _ = InputMethodDictionaryGroupProvider.LoadFromOptionsAsync(options, default);
            }
        }
    }
}
