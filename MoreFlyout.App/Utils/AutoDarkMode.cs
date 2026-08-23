namespace MoreFlyout.App.Utils;

internal static class AutoDarkMode
{
    public static bool IsAutoDarkModeInstalled()
    {
        var localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        var commonPath = Path.Combine(localAppData, "Programs", "AutoDarkMode");
        if (Directory.Exists(commonPath))
        {
            return true;
        }

        string[] registryPaths = [@"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall", @"SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall"];

        Microsoft.Win32.RegistryKey[] roots = [Microsoft.Win32.Registry.LocalMachine, Microsoft.Win32.Registry.CurrentUser];

        foreach (var root in roots)
        {
            foreach (var path in registryPaths)
            {
                using var key = root.OpenSubKey(path);
                if (key is null)
                    continue;

                foreach (var subKeyName in key.GetSubKeyNames())
                {
                    using var subKey = key.OpenSubKey(subKeyName);
                    if (subKey?.GetValue("DisplayName") is string displayName && displayName.Contains("Auto Dark Mode", StringComparison.OrdinalIgnoreCase))
                    {
                        return true;
                    }
                }
            }
        }

        return false;
    }
}
