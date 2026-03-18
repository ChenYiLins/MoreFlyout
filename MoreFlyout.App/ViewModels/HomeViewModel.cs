using System.Diagnostics;
using MoreFlyout.Comms;

namespace MoreFlyout.App.ViewModels;

public partial class HomeViewModel : ObservableObject
{
    [ObservableProperty]
    public partial bool MoreFlyoutEnabled { get; set; }

    public HomeViewModel()
    {
        LoadConfig();
    }

    private void LoadConfig()
    {
        MoreFlyoutEnabled = ConfigManager.Instance.ServiceSettings.IsEnabled;
    }

    async partial void OnMoreFlyoutEnabledChanged(bool value)
    {
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
                            FileName = Path.Combine(directoryInfo.FullName, "server","MoreFlyout.Server.exe"),
                            UseShellExecute = false,
                            CreateNoWindow = true,
                        }
                    );
                }
                catch (Exception e)
                {
                    MoreFlyoutEnabled = false;
                }
            }
        }
        else
        {
            bool success = await PipeClient.SendMessageAsync(new Message() { Type = MessageType.StopServer });
            if (!success)
            {
                MoreFlyoutEnabled = true;
            }
        }

        ConfigManager.Instance.ServiceSettings.IsEnabled = MoreFlyoutEnabled;
        ConfigManager.Save();
    }
}
