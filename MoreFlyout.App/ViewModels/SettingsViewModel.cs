using CommunityToolkit.Mvvm.Input;
using MoreFlyout.Comms;

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

        var autoStartResponse = await PipeClient.SendMessageAndGetReplyAsync(new Message() { Type = MessageType.QueryAutoStart });
        AutoStartPath = autoStartResponse?.Content;

        await Task.Delay(1000);

        ProgressAutostartDetailsVisibility = Visibility.Collapsed;
    }

    public SettingsViewModel()
    {
        _ = LoadConfig();
    }

    private async Task LoadConfig()
    {
        StartWithWindowsEnabled = ConfigManager.Instance.ServiceSettings.AutoStart;
        HideTrayIconEnabled = !ConfigManager.Instance.ServiceSettings.ShowTrayIcon;
        var autoStartResponse = await PipeClient.SendMessageAndGetReplyAsync(new Message() { Type = MessageType.QueryAutoStart });
        AutoStartPath = autoStartResponse?.Content;
    }

    async partial void OnStartWithWindowsEnabledChanged(bool value)
    {
        if (value)
        {
            bool success = await PipeClient.SendMessageAsync(new Message() { Type = MessageType.EnableAutoStart });
            if (!success)
            {
                StartWithWindowsEnabled = false;
            }
        }
        else
        {
            bool success = await PipeClient.SendMessageAsync(new Message() { Type = MessageType.DisableAutoStart });
            if (!success)
            {
                StartWithWindowsEnabled = true;
            }
        }

        ConfigManager.Instance.ServiceSettings.AutoStart = StartWithWindowsEnabled;
        ConfigManager.Save();
    }

    async partial void OnHideTrayIconEnabledChanged(bool value)
    {
        if (value)
        {
            bool success = await PipeClient.SendMessageAsync(new Message() { Type = MessageType.DisableTrayIcon });
            if (!success)
            {
                HideTrayIconEnabled = true;
            }
        }
        else
        {
            bool success = await PipeClient.SendMessageAsync(new Message() { Type = MessageType.EnableTrayIcon });
            if (!success)
            {
                HideTrayIconEnabled = false;
            }
        }

        ConfigManager.Instance.ServiceSettings.ShowTrayIcon = !HideTrayIconEnabled;
        ConfigManager.Save();
    }
}
