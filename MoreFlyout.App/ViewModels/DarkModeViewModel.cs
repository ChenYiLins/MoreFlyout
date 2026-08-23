using MoreFlyout.App.Utils;

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
            AutoDarkModeInstalled = AutoDarkMode.IsAutoDarkModeInstalled();
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

    partial void OnDarkModeFlyoutEnabledChanged(bool value)
    {
        if (_isLoading)
        {
            return;
        }

        ConfigManager.Instance.DarkModeFlyoutSettings.IsEnabled = value;
        ConfigManager.Save();
    }

    partial void OnFlyoutTimeoutValueChanged(int value)
    {
        if (_isLoading)
        {
            return;
        }

        ConfigManager.Instance.DarkModeFlyoutSettings.TimeoutHiding = value;
        ConfigManager.Save();
    }
}
