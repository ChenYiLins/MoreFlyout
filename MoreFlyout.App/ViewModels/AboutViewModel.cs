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

    public AboutViewModel()
    {
        Server = VersionInfo.Service;
        AppVersion = VersionInfo.App;
        NetCore = VersionInfo.NetCore;
        Windows = VersionInfo.WindowsVersion;
        Arch = VersionInfo.Architecture;
    }
}
