using System.Diagnostics;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Windowing;
using MoreFlyout.App.Contracts.Services;
using Windows.Devices.Display;
using Windows.Devices.Enumeration;
using Windows.Graphics;

namespace MoreFlyout.App.Services;

public class ActivationService(INavigationService navigationService) : IActivationService
{
    public async Task ActivateAsync(object activationArgs)
    {
        // Navigate to default page
        navigationService.NavigateTo(typeof(HomeViewModel).FullName!);

        // Move window to config position
        await MoveWindowAsync();

        // Activate the MainWindow.
        App.MainWindow.Activate();
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

    private static async Task WaitForXamlRootAsync()
    {
        var tcs = new TaskCompletionSource();

        int attempts = 0;
        const int maxAttempts = 50;
        const int delayMs = 50;

        DispatcherQueue
            .GetForCurrentThread()
            .TryEnqueue(async () =>
            {
                while (attempts < maxAttempts)
                {
                    if (App.MainWindow.Content?.XamlRoot != null)
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
        if (!Debugger.IsAttached)
        {
            using Mutex serviceRunning = new(false, "330f929b-ac7a-4791-9958-f8b9268ca35d");
            if (serviceRunning.WaitOne(TimeSpan.FromMilliseconds(100), false))
            {
                using Process svc = new();
                svc.StartInfo.UseShellExecute = false;
                //svc.StartInfo.FileName = Helper.ExecutionPathService;
                svc.StartInfo.CreateNoWindow = true;
                svc.Start();
                serviceRunning.ReleaseMutex();
                return true;
            }
        }
        return false;
    }

    private static async Task<bool> VerifyServiceStartupAsync()
    {
        //const int maxRetries = 5;
        //ApiResponse response = null!;
        //for (int i = 0; i < maxRetries; i++)
        //{
        //    response = await Task.Run(() => ApiResponse.FromString(MessageHandler<,>.Client.SendMessageAndGetReply(Command.Alive)));
        //    if (response.StatusCode == StatusCode.Ok)
        //        break;
        //    await Task.Delay(1000);
        //}

        //if (response.StatusCode == StatusCode.Timeout)
        //{
        //    return false;
        //}
        //else
        //{
        //    return true;
        //}
        return true;
    }
}
