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
        DarkModeFlyoutEnabled = ConfigManager.Instance.DarkModeFlyout.IsEnabled;
        FlyoutTimeoutValue = ConfigManager.Instance.DarkModeFlyout.TimeoutHiding;
    }

    partial void OnDarkModeFlyoutEnabledChanged(bool value)
    {
        ConfigManager.Instance.KeyIndicatorFlyout.IsEnabled = value;
        ConfigManager.Save();
    }

    partial void OnFlyoutTimeoutValueChanged(int value)
    {
        ConfigManager.Instance.KeyIndicatorFlyout.TimeoutHiding = value;
        ConfigManager.Save();
    }
}
