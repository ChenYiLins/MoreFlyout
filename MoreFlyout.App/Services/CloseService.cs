using Microsoft.UI.Windowing;
using MoreFlyout.App.Contracts.Services;

namespace MoreFlyout.App.Services;

public class CloseService : ICloseService
{
    public void Close()
    {
        EndFirstLanuch();

        SaveWindowConfig();
    }

    private static void EndFirstLanuch()
    {
        if (ConfigManager.Instance.AppSettings.FirstLaunch)
        {
            ConfigManager.Instance.AppSettings.FirstLaunch = false;
            ConfigManager.Save();
        }
    }

    private static void SaveWindowConfig()
    {
        if (App.MainWindow.AppWindow.Presenter is OverlappedPresenter presenter)
        {
            ConfigManager.Instance.AppSettings.WindowState = presenter.State switch
            {
                OverlappedPresenterState.Maximized => 0,
                OverlappedPresenterState.Minimized => 1,
                OverlappedPresenterState.Restored => 2,
                _ => 2,
            };

            if(presenter.State == OverlappedPresenterState.Restored)
            {
                var windowPositonAndSize = new int[4];
                windowPositonAndSize[0] = App.MainWindow.AppWindow.Position.X;
                windowPositonAndSize[1] = App.MainWindow.AppWindow.Position.Y;
                windowPositonAndSize[2] = App.MainWindow.AppWindow.Size.Width;
                windowPositonAndSize[3] = App.MainWindow.AppWindow.Size.Height;
                ConfigManager.Instance.AppSettings.WindowPositionAndSize = windowPositonAndSize;
            }
        }

        ConfigManager.Save();
    }
}
