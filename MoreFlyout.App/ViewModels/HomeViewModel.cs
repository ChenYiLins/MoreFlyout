using System.Diagnostics;
using MoreFlyout.App.Contracts.Services;
using MoreFlyout.Comms;

namespace MoreFlyout.App.ViewModels;

public partial class HomeViewModel : ObservableObject
{
    private bool _isLoading;
    private readonly IErrorService _errorService;

    [ObservableProperty]
    public partial bool MoreFlyoutEnabled { get; set; }

    public HomeViewModel(IErrorService errorService)
    {
        _errorService = errorService;

        LoadConfig();
    }

    private void LoadConfig()
    {
        _isLoading = true;
        try
        {
            MoreFlyoutEnabled = ConfigManager.Instance.ServiceSettings.IsEnabled;
        }
        finally
        {
            _isLoading = false;
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
                            FileName = Path.Combine(directoryInfo.FullName, "server", "MoreFlyout.Server.exe"),
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
            var success = await PipeClient.SendMessageAsync(new Message() { Type = MessageType.StopServer });
            if (!success)
            {
                MoreFlyoutEnabled = true;
            }
        }

        ConfigManager.Instance.ServiceSettings.IsEnabled = MoreFlyoutEnabled;
        ConfigManager.Save();
    }
}
