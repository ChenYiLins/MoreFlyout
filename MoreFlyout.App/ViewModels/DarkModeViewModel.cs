namespace MoreFlyout.App.ViewModels;

public partial class DarkModeViewModel : ObservableObject
{
    private bool _isLoading;

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
