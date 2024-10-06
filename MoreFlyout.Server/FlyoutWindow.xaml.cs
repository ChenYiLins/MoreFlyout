using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.UI.WindowsAndMessaging;

namespace MoreFlyout.Server;

public sealed partial class FlyoutWindow : WindowEx
{
    public FlyoutWindow()
    {
        InitializeComponent();
        Content = null;

        // Change FlyoutWindow style
        var hWnd = (HWND)WinRT.Interop.WindowNative.GetWindowHandle(this);
        _ = PInvoke.SetWindowLong(hWnd, WINDOW_LONG_PTR_INDEX.GWL_EXSTYLE, PInvoke.GetWindowLong(hWnd, WINDOW_LONG_PTR_INDEX.GWL_EXSTYLE) | (int)WINDOW_EX_STYLE.WS_EX_NOACTIVATE);
        this.SetWindowOpacity(0);

        // Set FlyoutWindow position
        var screenWidth = PInvoke.GetSystemMetrics(SYSTEM_METRICS_INDEX.SM_CXSCREEN);
        var screenHeight = PInvoke.GetSystemMetrics(SYSTEM_METRICS_INDEX.SM_CYSCREEN);
        var dpiForWindow = PInvoke.GetDpiForWindow(hWnd);
        var windowRatio = dpiForWindow / 96.0;
        AppWindow.Resize(new Windows.Graphics.SizeInt32((int)(195 * windowRatio), (int)(48 * windowRatio)));
        AppWindow.Move(new Windows.Graphics.PointInt32(screenWidth / 2 - (int)(195 / 2 * windowRatio), screenHeight - (int)(86 * windowRatio)));
    }
}

