namespace MoreFlyout.App.ViewModels;

public partial class KeyIndicatorViewModel : ObservableObject
{
    [ObservableProperty]
    public partial bool KeyIndicatorFlyoutEnabled { get; set; }

    [ObservableProperty]
    public partial int FlyoutTimeoutValue { get; set; }

    public KeyIndicatorViewModel()
    {
        LoadConfig();
    }

    private void LoadConfig()
    {
        KeyIndicatorFlyoutEnabled = ConfigManager.Instance.KeyIndicatorFlyoutSettings.IsEnabled;
        FlyoutTimeoutValue = ConfigManager.Instance.KeyIndicatorFlyoutSettings.TimeoutHiding;
    }

    partial void OnKeyIndicatorFlyoutEnabledChanged(bool value)
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
