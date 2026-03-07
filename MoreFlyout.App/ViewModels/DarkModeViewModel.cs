namespace MoreFlyout.App.ViewModels;

public partial class DarkModeViewModel: ObservableObject
{
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
        DarkModeFlyoutEnabled = ConfigManager.Instance.DarkModeFlyoutSettings.IsEnabled;
        FlyoutTimeoutValue = ConfigManager.Instance.DarkModeFlyoutSettings.TimeoutHiding;
    }

    partial void OnDarkModeFlyoutEnabledChanged(bool value)
    {
        ConfigManager.Instance.KeyIndicatorFlyoutSettings.IsEnabled = value;
        ConfigManager.Save();
    }

    partial void OnFlyoutTimeoutValueChanged(int value)
    {
        ConfigManager.Instance.KeyIndicatorFlyoutSettings.TimeoutHiding = value;
        ConfigManager.Save();
    }
}
