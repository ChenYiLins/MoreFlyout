using CommunityToolkit.Mvvm.Input;
using MoreFlyout.App.Utils;

namespace MoreFlyout.App.ViewModels;

public partial class AboutViewModel : ObservableObject
{
    [ObservableProperty]
    public partial string Server { get; set; }

    [ObservableProperty]
    public partial string AppVersion { get; set; }

    [ObservableProperty]
    public partial string NetCore { get; set; }

    [ObservableProperty]
    public partial string Windows { get; set; }

    [ObservableProperty]
    public partial string Arch { get; set; }

    [RelayCommand]
    private void OpenLogFile()
    {
        var logDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), @"MoreFlyout/service.log");
        if (File.Exists(logDir))
        {
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo { FileName = logDir, UseShellExecute = true });
        }
    }

    public AboutViewModel()
    {
        Server = VersionInfo.Service;
        AppVersion = VersionInfo.App;
        NetCore = VersionInfo.NetCore;
        Windows = VersionInfo.WindowsVersion;
        Arch = VersionInfo.Architecture;
    }
}
