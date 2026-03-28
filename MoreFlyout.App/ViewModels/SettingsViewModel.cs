using CommunityToolkit.Mvvm.Input;
using MoreFlyout.Comms;

namespace MoreFlyout.App.ViewModels;

public partial class SettingsViewModel : ObservableObject
{
    private bool _isLoading;

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
        _isLoading = true;
        try
        {
            StartWithWindowsEnabled = ConfigManager.Instance.ServiceSettings.AutoStart;
            HideTrayIconEnabled = ConfigManager.Instance.ServiceSettings.HideTrayIcon;
            var autoStartResponse = await PipeClient.SendMessageAndGetReplyAsync(new Message() { Type = MessageType.QueryAutoStart });
            AutoStartPath = autoStartResponse?.Content;
        }
        finally
        {
            _isLoading = false;
        }
    }

    async partial void OnStartWithWindowsEnabledChanged(bool value)
    {
        if (_isLoading)
        {
            return;
        }

        if (value)
        {
            var success = await PipeClient.SendMessageAsync(new Message() { Type = MessageType.EnableAutoStart });
            if (!success)
            {
                StartWithWindowsEnabled = false;
            }
        }
        else
        {
            var success = await PipeClient.SendMessageAsync(new Message() { Type = MessageType.DisableAutoStart });
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
        if (_isLoading)
        {
            return;
        }

        if (value)
        {
            var success = await PipeClient.SendMessageAsync(new Message() { Type = MessageType.DisableTrayIcon });
            if (!success)
            {
                HideTrayIconEnabled = false;
            }
        }
        else
        {
            var success = await PipeClient.SendMessageAsync(new Message() { Type = MessageType.EnableTrayIcon });
            if (!success)
            {
                HideTrayIconEnabled = true;
            }
        }

        ConfigManager.Instance.ServiceSettings.HideTrayIcon = HideTrayIconEnabled;
        ConfigManager.Save();
    }
}
