namespace MoreFlyout.App.ViewModels;

public partial class MediaViewModel : ObservableObject
{
    private bool _isLoading;

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
        _isLoading = true;
        try
        {
            MediaFlyoutEnabled = ConfigManager.Instance.MediaFlyoutSettings.IsEnabled;
            FlyoutTimeoutValue = ConfigManager.Instance.MediaFlyoutSettings.TimeoutHiding;
        }
        finally
        {
            _isLoading = false;
        }
    }

    partial void OnMediaFlyoutEnabledChanged(bool value)
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
