using Microsoft.Win32;

namespace MoreFlyout.Server.Utils;

internal class SystemTheme
{
    public static ElementTheme GetCurrentSystemTheme()
    {
        try
        {
            using var key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize");
            var value = key?.GetValue("SystemUsesLightTheme");
            if (value is int intValue && intValue == 1)
            {
                return ElementTheme.Light;
            }
            else
            {
                return ElementTheme.Dark;
            }
        }
        catch
        {
            return ElementTheme.Dark;
        }
    }

    public static bool IsTaskbarColorPrevalenceEnabled()
    {
        const string keyPath = @"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize";
        using var key = Registry.CurrentUser.OpenSubKey(keyPath);
        var value = key?.GetValue("ColorPrevalence");
        return value is int v && v != 0;
    }
}
