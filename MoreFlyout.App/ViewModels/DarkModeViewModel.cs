using CommunityToolkit.Mvvm.Input;

namespace MoreFlyout.App.ViewModels;

public partial class DarkModeViewModel : ObservableObject
{
    private bool _isLoading;

    [ObservableProperty]
    public partial bool AutoDarkModeInstalled { get; set; }

    [ObservableProperty]
    public partial bool DarkModeFlyoutEnabled { get; set; }

    [ObservableProperty]
    public partial int FlyoutTimeoutValue { get; set; }

    public DarkModeViewModel()
    {
        LoadConfig();
    }

    private void LoadConfig()
    {
        _isLoading = true;
        try
        {
            AutoDarkModeInstalled = IsAutoDarkModeInstalled();
            if (!AutoDarkModeInstalled)
            {
                ConfigManager.Instance.DarkModeFlyoutSettings.IsEnabled = false;
                ConfigManager.Save();
            }

            DarkModeFlyoutEnabled = ConfigManager.Instance.DarkModeFlyoutSettings.IsEnabled;
            FlyoutTimeoutValue = ConfigManager.Instance.DarkModeFlyoutSettings.TimeoutHiding;
        }
        finally
        {
            _isLoading = false;
        }
    }

    private static bool IsAutoDarkModeInstalled()
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

    partial void OnDarkModeFlyoutEnabledChanged(bool value)
    {
        if (_isLoading)
        {
            return;
        }

        ConfigManager.Instance.KeyIndicatorFlyoutSettings.IsEnabled = value;
        ConfigManager.Save();
    }

    partial void OnFlyoutTimeoutValueChanged(int value)
    {
        if (_isLoading)
        {
            return;
        }

        ConfigManager.Instance.KeyIndicatorFlyoutSettings.TimeoutHiding = value;
        ConfigManager.Save();
    }
}
