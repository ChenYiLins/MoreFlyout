using CommunityToolkit.Mvvm.Input;

namespace MoreFlyout.App.ViewModels;

public partial class SettingsViewModel : ObservableObject
{
    [ObservableProperty]
    public partial bool StartWithWindowsEnabled { get; set; }

    [ObservableProperty]
    public partial Visibility ProgressAutostartDetailsVisibility { get; set; } = Visibility.Collapsed;

    [ObservableProperty]
    public partial string? AutoStartPath { get; set; }

    [ObservableProperty]
    public partial bool HideTrayIconEnabled { get; set; }

    [RelayCommand]
    private async Task RefreshAutoStartPath()
    {
        if (ProgressAutostartDetailsVisibility == Visibility.Collapsed)
        {
            ProgressAutostartDetailsVisibility = Visibility.Visible;
        }

        AutoStartPath = "WuuuuuWaaaa";

        await Task.Delay(1000);

        ProgressAutostartDetailsVisibility = Visibility.Collapsed;
    }

    public SettingsViewModel()
    {
        LoadConfig();
    }

    private void LoadConfig()
    {
        StartWithWindowsEnabled = ConfigManager.Instance.ServiceSettings.AutoStart;
        HideTrayIconEnabled = !ConfigManager.Instance.ServiceSettings.ShowTrayIcon;
    }
}
