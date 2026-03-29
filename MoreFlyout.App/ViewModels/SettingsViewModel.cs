using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using CommunityToolkit.Mvvm.Input;
using MoreFlyout.App.Contracts.Services;
using MoreFlyout.App.Helpers;
using MoreFlyout.App.Models;
using MoreFlyout.Comms;

namespace MoreFlyout.App.ViewModels;

public partial class SettingsViewModel : ObservableObject
{
    private bool _isLoading;
    private readonly IErrorService _errorService;
    private readonly Microsoft.UI.Dispatching.DispatcherQueue _dispatcherQueue;

    [ObservableProperty]
    public partial bool StartWithWindowsEnabled { get; set; }

    [ObservableProperty]
    public partial Visibility ProgressAutostartDetailsVisibility { get; set; } = Visibility.Collapsed;

    [ObservableProperty]
    public partial string? AutoStartPath { get; set; }

    [ObservableProperty]
    public partial bool HideTrayIconEnabled { get; set; }

    public ObservableCollection<LanguageOption> LanguageOptions { get; }

    [ObservableProperty]
    public partial string? SelectedLanguage { get; set; }

    [ObservableProperty]
    public partial bool IsLanguageChangedInfoBarOpen { get; set; }

    [RelayCommand]
    private async Task RefreshAutoStartPath()
    {
        if (ProgressAutostartDetailsVisibility == Visibility.Collapsed)
        {
            ProgressAutostartDetailsVisibility = Visibility.Visible;
        }

        try
        {
            var autoStartResponse = await PipeClient.SendMessageAndGetReplyAsync(new Message() { Type = MessageType.QueryAutoStart });
            AutoStartPath = autoStartResponse?.Content;
        }
        catch (Exception ex)
        {
            await _errorService.ShowErrorMessageAsync(ex, App.MainWindow.Content.XamlRoot, "SettingsViewModel");
        }

        await Task.Delay(1000);

        ProgressAutostartDetailsVisibility = Visibility.Collapsed;
    }

    [RelayCommand]
    private async Task RestartApp()
    {
        try
        {
            await PipeClient.SendMessageAsync(new Message() { Type = MessageType.RestartServer });
        }
        catch (Exception ex)
        {
            await _errorService.ShowErrorMessageAsync(ex, App.MainWindow.Content.XamlRoot, "SettingsViewModel");
            return;
        }

        App.ReleaseSingleInstanceMutex();
        Process.Start(new ProcessStartInfo(CommonHelper.ExecutionPathApp) { UseShellExecute = false });
        Application.Current.Exit();
    }

    [RelayCommand]
    private void OpenConfigFile()
    {
        try
        {
            new Process { StartInfo = new ProcessStartInfo(CommonHelper.ConfigPath) { UseShellExecute = true } }.Start();
        }
        catch (Exception ex)
        {
            _errorService.ShowErrorMessageAsync(ex, App.MainWindow.Content.XamlRoot, "SettingsViewModel");
        }
    }

    [RelayCommand]
    private void OpenConfigFolder()
    {
        try
        {
            new Process
            {
                StartInfo = new ProcessStartInfo()
                {
                    FileName = "cmd.exe",
                    Arguments = "/c start " + CommonHelper.ConfigFolderPath,
                    WindowStyle = ProcessWindowStyle.Hidden,
                },
            }.Start();
        }
        catch (Exception ex)
        {
            _errorService.ShowErrorMessageAsync(ex, App.MainWindow.Content.XamlRoot, "SettingsViewModel");
        }
    }

    public SettingsViewModel(IErrorService errorService)
    {
        _errorService = errorService;
        _dispatcherQueue = Microsoft.UI.Dispatching.DispatcherQueue.GetForCurrentThread();

        LanguageOptions = new ObservableCollection<LanguageOption>(
            LanguageHelper.SupportedCultures.Select(code =>
            {
                var culture = CultureInfo.GetCultureInfo(code);
                string native = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(culture.NativeName);
                string english = culture.EnglishName;
                return new LanguageOption
                {
                    DisplayName = $"{native} - {english}", // example: Deutsch (German)
                    CultureCode = code, // example: de, zh-hans
                };
            })
        );

        LoadConfig();
    }

    private void LoadConfig()
    {
        _isLoading = true;
        try
        {
            StartWithWindowsEnabled = ConfigManager.Instance.ServiceSettings.AutoStart;
            HideTrayIconEnabled = ConfigManager.Instance.ServiceSettings.HideTrayIcon;
            AutoStartPath = "Loading".GetLocalized();
            SelectedLanguage = LanguageHelper.GetDefaultLanguage();

            _dispatcherQueue.TryEnqueue(async () =>
            {
                var autoStartResponse = await PipeClient.SendMessageAndGetReplyAsync(new Message() { Type = MessageType.QueryAutoStart });
                if (string.IsNullOrEmpty(autoStartResponse?.Content))
                {
                    AutoStartPath = "Msg_PathNotFound".GetLocalized();
                }
                else
                {
                    AutoStartPath = autoStartResponse.Content;
                }
            });
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

    partial void OnSelectedLanguageChanged(string? value)
    {
        if (_isLoading)
        {
            return;
        }

        string currentCulture = Microsoft.Windows.Globalization.ApplicationLanguages.PrimaryLanguageOverride;
        bool isSameLanguage = string.Equals(currentCulture, value, StringComparison.OrdinalIgnoreCase);

        if (value is null)
        {
            return;
        }
        ConfigManager.Instance.AppSettings.SelectedLanguageCode = value;
        ConfigManager.Instance.ServiceSettings.SelectedLanguageCode = value;
        ConfigManager.Instance.AppSettings.LanguageChanged = !isSameLanguage;
        ConfigManager.Save();
        IsLanguageChangedInfoBarOpen = !isSameLanguage;
    }
}
