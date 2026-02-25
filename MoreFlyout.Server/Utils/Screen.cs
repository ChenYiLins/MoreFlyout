using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.UI.WindowsAndMessaging;

namespace MoreFlyout.Server.Utils;

internal class Screen
{
    public static unsafe bool IsFullScreenActive()
    {
        var isFullScreen = true;
        const int MAX_PATH = 260;
        Span<char> buffer = new char[MAX_PATH + 1];

        var screenWidth = PInvoke.GetSystemMetrics(SYSTEM_METRICS_INDEX.SM_CXSCREEN);
        var screenHeight = PInvoke.GetSystemMetrics(SYSTEM_METRICS_INDEX.SM_CYSCREEN);

        System.Drawing.Point[] screenCorners = [new(1, screenHeight - 1), new(screenWidth - 1, screenHeight - 1)];

        foreach (var corner in screenCorners)
        {
            var hWnd = PInvoke.WindowFromPoint(corner);

            fixed (char* pBuffer = buffer)
            {
                PWSTR pWSTR = pBuffer;
                _ = PInvoke.GetClassName(hWnd, pWSTR, MAX_PATH);
                if (pWSTR.ToString() == "Shell_TrayWnd" || pWSTR.ToString() == "TrayNotifyWnd")
                    isFullScreen = false;
            }
        }

        return isFullScreen;
    }
}
