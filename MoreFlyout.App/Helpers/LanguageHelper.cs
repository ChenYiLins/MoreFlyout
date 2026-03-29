using System.Globalization;
using Microsoft.Windows.Globalization;

namespace MoreFlyout.App.Helpers;

public static class LanguageHelper
{
    public static readonly string[] SupportedCultures =
    [
        // Left-to-Right (LTR) languages
        "en-US",
        "zh-Hans",
        "zh-Hant",
        // Right-to-Left (RTL) languages
    ];

    private static string SelectedLanguageCode { get; set; } = "en-US"; // equal to <DefaultLanguage>

    public static string GetDefaultLanguage()
    {
        var language = ConfigManager.Instance.AppSettings.SelectedLanguageCode;
        if (!string.IsNullOrEmpty(language) && SupportedCultures.Contains(language))
        {
            SelectedLanguageCode = language;
        }
        else
        {
            var preferredLanguages = ApplicationLanguages.Languages; // example: ["fr-FR", "en-US", "de-DE"]
            string topLanguage;
            if (preferredLanguages.Any())
            {
                topLanguage = preferredLanguages[0];
            }
            else // very unlikely, but just in case
            {
                topLanguage = CultureInfo.CurrentUICulture.Name;
            }

            if (SupportedCultures.Contains(topLanguage))
            {
                SelectedLanguageCode = topLanguage;
            }
            else
            {
                var topLanguageArray = topLanguage.Split('-');
                var neutralLanguage = string.Join("-", topLanguageArray[..^1]);
                if (SupportedCultures.Contains(neutralLanguage))
                {
                    SelectedLanguageCode = neutralLanguage;
                }
                // else keep the default "en-US"
            }
            ConfigManager.Instance.AppSettings.SelectedLanguageCode = SelectedLanguageCode;
            ConfigManager.Save();
        }
        return SelectedLanguageCode;
    }
}
