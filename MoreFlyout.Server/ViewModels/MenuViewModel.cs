using System.Diagnostics;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MoreFlyout.Server.Contracts.Services;

namespace MoreFlyout.Server.ViewModels;

public partial class MenuViewModel : ObservableRecipient
{
    private ILocalSettingsService _settingsService;

    [ObservableProperty]
    private bool _startWithWindows;

    public ICommand SwitchStartWithWindowsCommand
    {
        get;
    }

    public MenuViewModel(ILocalSettingsService localSettingsService)
    {
        _settingsService = localSettingsService;
        var readSettingTask = Task.Run(
            async () =>
            {
                _startWithWindows = await _settingsService.ReadSettingAsync<bool>("StartWithWindows");
            });
        readSettingTask.Wait();

        SwitchStartWithWindowsCommand = new RelayCommand<bool>(
            async (param) =>
            {
                _startWithWindows = param;
                await _settingsService.SaveSettingAsync<bool>("StartWithWindows", param);

                Microsoft.Win32.RegistryKey registryKey = Microsoft.Win32.Registry.CurrentUser;
                if (_startWithWindows)
                {
                    try
                    {
                        Microsoft.Win32.RegistryKey run = registryKey.CreateSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run");
                        run.SetValue("MoreFlyout.Server", "\"" + System.AppDomain.CurrentDomain.BaseDirectory + "MoreFlyout.Server.exe" + "\"");
                        run.Close();
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine(ex.Message);
                    }
                }
                else
                {
                    try
                    {
                        Microsoft.Win32.RegistryKey run = registryKey.CreateSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run");
                        run.DeleteValue("MoreFlyout.Server");
                        run.Close();
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine(ex.Message);
                    }
                }
                registryKey.Close();
            });
    }
}
