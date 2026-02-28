using CommunityToolkit.Mvvm.Input;
using MoreFlyout.Config;

namespace MoreFlyout.Server.ViewModels;

public partial class TrayIconMenuFlyoutViewModel : ObservableRecipient
{
    private bool _isInitializing;

    [ObservableProperty]
    public partial bool KeyIndicatorFlyoutEnabled { get; set; }

    [ObservableProperty]
    public partial bool MediaFlyoutEnabled { get; set; }

    [ObservableProperty]
    public partial bool DarkModeFlyoutEnabled { get; set; }

    [RelayCommand]
    private void OpenLogFile()
    {
        var logDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), @"MoreFlyout/service.log");
        if (File.Exists(logDir))
        {
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo { FileName = logDir, UseShellExecute = true });
        }
    }

    [RelayCommand]
    private void Close()
    {
        Environment.Exit(0);
    }

    public TrayIconMenuFlyoutViewModel()
    {
        InitializeProperties();
    }

    private void InitializeProperties()
    {
        _isInitializing = true;

        KeyIndicatorFlyoutEnabled = ConfigManager.Instance.KeyIndicatorFlyout.IsEnabled;
        MediaFlyoutEnabled = ConfigManager.Instance.MediaFlyout.IsEnabled;
        DarkModeFlyoutEnabled = ConfigManager.Instance.DarkModeFlyout.IsEnabled;

        _isInitializing = false;
    }

    partial void OnKeyIndicatorFlyoutEnabledChanged(bool value)
    {
        if (_isInitializing)
        {
            return;
        }
        ConfigManager.Instance.KeyIndicatorFlyout.IsEnabled = value;
        ConfigManager.Save();
    }

    partial void OnMediaFlyoutEnabledChanged(bool value)
    {
        if (_isInitializing)
        {
            return;
        }
        ConfigManager.Instance.MediaFlyout.IsEnabled = value;
        ConfigManager.Save();
    }

    partial void OnDarkModeFlyoutEnabledChanged(bool value)
    {
        if (_isInitializing)
        {
            return;
        }
        ConfigManager.Instance.DarkModeFlyout.IsEnabled = value;
        ConfigManager.Save();
    }
}
