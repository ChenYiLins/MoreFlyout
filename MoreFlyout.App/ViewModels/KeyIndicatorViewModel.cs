namespace MoreFlyout.App.ViewModels;

public partial class KeyIndicatorViewModel : ObservableObject
{
    private bool _isLoading;

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
        _isLoading = true;
        try
        {
            KeyIndicatorFlyoutEnabled = ConfigManager.Instance.KeyIndicatorFlyoutSettings.IsEnabled;
            FlyoutTimeoutValue = ConfigManager.Instance.KeyIndicatorFlyoutSettings.TimeoutHiding;
        }
        finally
        {
            _isLoading = false;
        }
    }

    partial void OnKeyIndicatorFlyoutEnabledChanged(bool value)
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
