using System.Collections.ObjectModel;
using System.Diagnostics;
using CommunityToolkit.Mvvm.Input;
using MoreFlyout.App.Contracts.Services;
using MoreFlyout.App.Helpers;
using MoreFlyout.App.Models;
using MoreFlyout.App.Utils;
using MoreFlyout.Comms;

namespace MoreFlyout.App.ViewModels;

public partial class HomeViewModel : ObservableObject
{
    private bool _isLoading;
    private readonly IErrorService _errorService;
    private readonly INavigationService _navigationService;

    [ObservableProperty]
    public partial string? ServiceStatusMessage { get; set; }

    [ObservableProperty]
    public partial InfoBarSeverity ServiceStatusSeverity { get; set; } = InfoBarSeverity.Informational;

    [ObservableProperty]
    public partial bool MoreFlyoutEnabled { get; set; }

    public ObservableCollection<SettingsGroupItem> ModuleItems { get; set; } = null!;

    [RelayCommand]
    public void NavigateToPages(Type tag)
    {
        _navigationService.NavigateTo(tag.FullName!);
    }

    public HomeViewModel(IErrorService errorService, INavigationService navigationService)
    {
        _errorService = errorService;
        _navigationService = navigationService;

        LoadConfig();
    }

    private void LoadConfig()
    {
        _isLoading = true;
        try
        {
            _ = LoadServiceStatusAsync();

            MoreFlyoutEnabled = ConfigManager.Instance.ServiceSettings.IsEnabled;

            ModuleItems =
            [
                new SettingsGroupItem()
                {
                    Label = "keyIndicator".GetLocalized(),
                    Icon = new FontIcon() { Glyph = "\xE92E" },
                    IsLocked = false,
                    IsEnabled = ConfigManager.Instance.KeyIndicatorFlyoutSettings.IsEnabled,
                    IsNew = false,
                    ClickCommand = NavigateToPagesCommand,
                    Tag = typeof(KeyIndicatorViewModel),
                    EnabledChangedCallback = OnModuleItemsEnabledChanged,
                },
                new SettingsGroupItem()
                {
                    Label = "Media".GetLocalized(),
                    Icon = new FontIcon() { Glyph = "\xE8D6" },
                    IsLocked = false,
                    IsEnabled = ConfigManager.Instance.MediaFlyoutSettings.IsEnabled,
                    IsNew = false,
                    ClickCommand = NavigateToPagesCommand,
                    Tag = typeof(MediaViewModel),
                    EnabledChangedCallback = OnModuleItemsEnabledChanged,
                },
                new SettingsGroupItem()
                {
                    Label = "AutoDarkMode".GetLocalized(),
                    Icon = new FontIcon() { Glyph = "\xE793" },
                    IsLocked = !AutoDarkMode.IsAutoDarkModeInstalled(),
                    IsEnabled = ConfigManager.Instance.DarkModeFlyoutSettings.IsEnabled,
                    IsNew = true,
                    ClickCommand = NavigateToPagesCommand,
                    Tag = typeof(DarkModeViewModel),
                    EnabledChangedCallback = OnModuleItemsEnabledChanged,
                },
            ];
        }
        finally
        {
            _isLoading = false;
        }
    }

    private async Task LoadServiceStatusAsync()
    {
        try
        {
            ServiceStatusMessage = "Msg_CheckingServiceStatus".GetLocalized();
            ServiceStatusSeverity = InfoBarSeverity.Informational;

            bool serviceRunning = await ServiceStatus.VerifyServiceRunningAsync();
            if (serviceRunning)
            {
                ServiceStatusMessage = "Msg_ServiceRunning".GetLocalized();
                ServiceStatusSeverity = InfoBarSeverity.Success;
            }
            else
            {
                if (ServiceStatus.VerifyServicePath())
                {
                    ServiceStatusMessage = "Msg_ServiceNotRunning".GetLocalized();
                    ServiceStatusSeverity = InfoBarSeverity.Warning;
                }
                else
                {
                    ServiceStatusMessage = "Msg_PathNotFound".GetLocalized();
                    ServiceStatusSeverity = InfoBarSeverity.Error;
                }
            }
        }
        catch (Exception ex)
        {
            _ = _errorService.ShowErrorMessageAsync(ex, App.MainWindow.Content.XamlRoot, "HomeViewModel");
        }
    }

    async partial void OnMoreFlyoutEnabledChanged(bool value)
    {
        if (_isLoading)
        {
            return;
        }

        if (value)
        {
            var directoryInfo = new DirectoryInfo(Environment.CurrentDirectory).Parent;
            if (directoryInfo is not null)
            {
                try
                {
                    Process.Start(
                        new ProcessStartInfo()
                        {
                            FileName = Path.Combine(directoryInfo.FullName, "service", "MoreFlyout.Service.exe"),
                            UseShellExecute = false,
                            CreateNoWindow = true,
                        }
                    );
                }
                catch (Exception ex)
                {
                    MoreFlyoutEnabled = false;
                    await _errorService.ShowErrorMessageAsync(ex, App.MainWindow.Content.XamlRoot, "HomeViewModel");
                }
            }
        }
        else
        {
            var success = await PipeClient.SendMessageAsync(new Message() { Type = MessageType.StopService });
            if (!success)
            {
                MoreFlyoutEnabled = true;
            }
        }

        ConfigManager.Instance.ServiceSettings.IsEnabled = MoreFlyoutEnabled;
        ConfigManager.Save();

        _ = LoadServiceStatusAsync();
    }

    private void OnModuleItemsEnabledChanged(SettingsGroupItem changedItem)
    {
        if (_isLoading)
        {
            return;
        }

        if (changedItem.Tag is Type tag)
        {
            if (tag == typeof(KeyIndicatorViewModel))
            {
                ConfigManager.Instance.KeyIndicatorFlyoutSettings.IsEnabled = changedItem.IsEnabled;
            }
            else if (tag == typeof(MediaViewModel))
            {
                ConfigManager.Instance.MediaFlyoutSettings.IsEnabled = changedItem.IsEnabled;
            }
            else if (tag == typeof(DarkModeViewModel))
            {
                ConfigManager.Instance.DarkModeFlyoutSettings.IsEnabled = changedItem.IsEnabled;
            }
        }

        ConfigManager.Save();
    }
}
