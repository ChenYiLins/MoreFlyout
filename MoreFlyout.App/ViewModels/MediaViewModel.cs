namespace MoreFlyout.App.ViewModels;

public partial class MediaViewModel: ObservableObject
{
    [ObservableProperty]
    public partial bool MediaFlyoutEnabled { get; set; }

    [ObservableProperty]
    public partial int FlyoutTimeoutValue { get; set; }

    public MediaViewModel()
    {
        LoadConfig();
    }

    private void LoadConfig()
    {
        MediaFlyoutEnabled = ConfigManager.Instance.MediaFlyoutSettings.IsEnabled;
        FlyoutTimeoutValue = ConfigManager.Instance.MediaFlyoutSettings.TimeoutHiding;
    }

    partial void OnMediaFlyoutEnabledChanged(bool value)
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
