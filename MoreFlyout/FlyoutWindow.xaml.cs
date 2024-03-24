using Microsoft.UI.Composition.SystemBackdrops;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using MoreFlyout.Views;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.UI.WindowsAndMessaging;
using WinRT;

namespace MoreFlyout;

public sealed partial class FlyoutWindow : WindowEx
{
    private readonly Microsoft.UI.Dispatching.DispatcherQueue dispatcherQueue;

    public FlyoutWindow()
    {
        InitializeComponent();
        Content = null;

        // Get dispatcherQueue
        dispatcherQueue = Microsoft.UI.Dispatching.DispatcherQueue.GetForCurrentThread();

        // Change FlyoutWindow style
        var presenter = this.AppWindow.Presenter as OverlappedPresenter;
        presenter!.IsResizable = false;
        presenter!.SetBorderAndTitleBar(false, false);
        this.SetWindowOpacity(0);

        // Set FlyoutWindow position
        var hWnd = (HWND)WinRT.Interop.WindowNative.GetWindowHandle(this);
        var screenWidth = PInvoke.GetSystemMetrics(SYSTEM_METRICS_INDEX.SM_CXSCREEN);
        var screenHeight = PInvoke.GetSystemMetrics(SYSTEM_METRICS_INDEX.SM_CYSCREEN);
        var dpiForWindow = PInvoke.GetDpiForWindow(hWnd);
        var windowRatio = dpiForWindow / 96.0;
        this.AppWindow.Resize(new Windows.Graphics.SizeInt32((int)(195 * windowRatio), (int)(48 * windowRatio)));
        this.AppWindow.Move(new Windows.Graphics.PointInt32(screenWidth / 2 - (int)(195 / 2 * windowRatio), screenHeight - (int)(56 * windowRatio)));
    }
}

