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
        PInvoke.SetWindowLongPtr(hWnd, WINDOW_LONG_PTR_INDEX.GWL_EXSTYLE, PInvoke.GetWindowLongPtr(hWnd, WINDOW_LONG_PTR_INDEX.GWL_EXSTYLE) | (nint)WINDOW_EX_STYLE.WS_EX_NOACTIVATE);
        this.SetWindowOpacity(0);

        // Set FlyoutWindow position
        var screenWidth = PInvoke.GetSystemMetrics(SYSTEM_METRICS_INDEX.SM_CXSCREEN);
        var screenHeight = PInvoke.GetSystemMetrics(SYSTEM_METRICS_INDEX.SM_CYSCREEN);
        var dpiForWindow = PInvoke.GetDpiForWindow(hWnd);
        var windowRatio = dpiForWindow / 96.0;
        this.AppWindow.Resize(new Windows.Graphics.SizeInt32((int)(195 * windowRatio), (int)(48 * windowRatio)));
        this.AppWindow.Move(new Windows.Graphics.PointInt32(screenWidth / 2 - (int)(195 / 2 * windowRatio), screenHeight - (int)(86 * windowRatio)));
    }
}

