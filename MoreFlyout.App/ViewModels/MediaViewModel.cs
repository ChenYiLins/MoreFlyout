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
        MediaFlyoutEnabled = ConfigManager.Instance.MediaFlyout.IsEnabled;
        FlyoutTimeoutValue = ConfigManager.Instance.MediaFlyout.TimeoutHiding;
    }

    partial void OnMediaFlyoutEnabledChanged(bool value)
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
