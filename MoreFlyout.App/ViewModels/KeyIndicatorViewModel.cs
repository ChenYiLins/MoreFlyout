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
        KeyIndicatorFlyoutEnabled = ConfigManager.Instance.KeyIndicatorFlyout.IsEnabled;
        FlyoutTimeoutValue = ConfigManager.Instance.KeyIndicatorFlyout.TimeoutHiding;
    }

    partial void OnKeyIndicatorFlyoutEnabledChanged(bool value)
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
