using System.Diagnostics;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Windowing;
using MoreFlyout.App.Contracts.Services;
using MoreFlyout.App.Helpers;
using MoreFlyout.App.Utils;
using MoreFlyout.Comms;
using Windows.Devices.Display;
using Windows.Devices.Enumeration;
using Windows.Graphics;

namespace MoreFlyout.App.Services;

public class ActivationService(INavigationService navigationService) : IActivationService
{
    private ContentDialog? _serviceInfoDialog;

    public async Task ActivateAsync(object activationArgs)
    {
        // Navigate to default page
        navigationService.NavigateTo(typeof(HomeViewModel).FullName!);

        // Move window to config position
        await MoveWindowAsync();

        // Activate the MainWindow.
        App.MainWindow.Activate();

        // Start Service and check result
        var serviceIssued = StartService();
        if (serviceIssued)
        {
            await InitalizeServiceInfoDialogAsync();
        }

        // Verify Service is running
        if(ConfigManager.Instance.ServiceSettings.IsEnabled)
        {
            var serviceVerified = await ServiceStatus.VerifyServiceStartupAsync();
            if (serviceIssued && !serviceVerified)
            {
                ShowUnresponsiveInfo();
            }
            else if (serviceIssued && serviceVerified)
            {
                _serviceInfoDialog?.Hide();
            }
        }
    }

    private static async Task MoveWindowAsync()
    {
        if (ConfigManager.Instance.AppSettings.FirstLaunch)
        {
            var screenSize = await GetMonitorSize();
            App.MainWindow.AppWindow.MoveAndResize(new Windows.Graphics.RectInt32(screenSize.Width / 2 - 1100 / 2, screenSize.Height / 2 - 650 / 2, 1100, 650));
        }
        else
        {
            var left = ConfigManager.Instance.AppSettings.WindowPositionAndSize[0];
            var top = ConfigManager.Instance.AppSettings.WindowPositionAndSize[1];
            var width = ConfigManager.Instance.AppSettings.WindowPositionAndSize[2];
            var height = ConfigManager.Instance.AppSettings.WindowPositionAndSize[3];
            App.MainWindow.AppWindow.MoveAndResize(new Windows.Graphics.RectInt32(left, top, width, height));
            var windowState = ConfigManager.Instance.AppSettings.WindowState;
            if (App.MainWindow.AppWindow.Presenter is OverlappedPresenter presenter)
            {
                var state = (OverlappedPresenterState)windowState;
                if (state == OverlappedPresenterState.Maximized)
                {
                    presenter.Maximize();
                }
            }
        }
    }

    private static async Task<SizeInt32> GetMonitorSize()
    {
        var monitorSize = new SizeInt32();
        var displayList = await DeviceInformation.FindAllAsync(DisplayMonitor.GetDeviceSelector());

        if (displayList.Count == 0)
        {
            return monitorSize;
        }

        var monitorInfo = await DisplayMonitor.FromInterfaceIdAsync(displayList[0].Id);
        if (monitorInfo is not null)
        {
            monitorSize = monitorInfo.NativeResolutionInRawPixels;
        }

        return monitorSize;
    }

    private async Task InitalizeServiceInfoDialogAsync()
    {
        await WaitForXamlRootAsync();

        _serviceInfoDialog = new ContentDialog
        {
            Title = "LaunchingService".GetLocalized(),
            Content = new StackPanel
            {
                Orientation = Orientation.Vertical,
                Spacing = 32,
                Children =
                {
                    new TextBlock { Text = "Msg_NoService".GetLocalized() },
                    new ProgressBar { IsIndeterminate = true },
                },
            },
            XamlRoot = App.MainWindow.Content.XamlRoot,
        };

        DispatcherQueue.GetForCurrentThread().TryEnqueue(async () => await _serviceInfoDialog.ShowAsync());
    }

    private void ShowUnresponsiveInfo()
    {
        _serviceInfoDialog?.Content = new StackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 32,
            Children =
            {
                new InfoBar
                {
                    Title = "ErrorOccurred".GetLocalized(),
                    Severity = InfoBarSeverity.Error,
                    IsOpen = true,
                    IsClosable = false,
                    Message = "Msg_ServiceUnresponsive".GetLocalized(),
                },
            },
        };
    }

    private static async Task WaitForXamlRootAsync()
    {
        var tcs = new TaskCompletionSource();

        var attempts = 0;
        const int maxAttempts = 50;
        const int delayMs = 50;

        DispatcherQueue
            .GetForCurrentThread()
            .TryEnqueue(async () =>
            {
                while (attempts < maxAttempts)
                {
                    if (App.MainWindow.Content?.XamlRoot is not null)
                    {
                        tcs.SetResult();
                        return;
                    }

                    attempts++;
                    await Task.Delay(delayMs);
                }

                tcs.SetException(new TimeoutException("MainWindow XamlRoot not available after waiting."));
            });

        await tcs.Task;
    }

    private static bool StartService()
    {
        if (Debugger.IsAttached)
        {
            return false;
        }

        using Mutex serviceRunning = new(false, "24043650-DED6-4E6B-8AFF-6BB03DFE3BDA");
        if (serviceRunning.WaitOne(TimeSpan.FromMilliseconds(100), false))
        {
            using Process svc = new();
            svc.StartInfo.UseShellExecute = false;
            svc.StartInfo.FileName = CommonHelper.ExecutionPathService;
            svc.StartInfo.CreateNoWindow = true;
            svc.Start();
            serviceRunning.ReleaseMutex();
            return true;
        }
        return false;
    }
}
