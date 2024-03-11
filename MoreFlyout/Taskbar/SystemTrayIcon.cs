using System.Drawing;
using System.Runtime.InteropServices;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.UI.Shell;
using Windows.Win32.UI.WindowsAndMessaging;

namespace MoreFlyout.Taskbar;

public class SystemTrayIcon : IDisposable
{
    // Constants
    public static TrayFlyoutContextMenu trayIconContextMenu;


    private const uint WM_FILES_UNIQUE_MESSAGE = 2048u;
    private const uint WM_FILES_CONTEXTMENU_DOCSLINK = 1u;
    private const uint WM_FILES_CONTEXTMENU_RESTART = 2u;
    private const uint WM_FILES_CONTEXTMENU_QUIT = 3u;

    // Fields

    private static readonly string PathApp = AppDomain.CurrentDomain.BaseDirectory;

    private static readonly Guid _trayIconGuid = new("9A16FE36-6683-4A4C-9A7C-D162B85493A4");

    private readonly SystemTrayIconWindow _IconWindow;

    private readonly uint _taskbarRestartMessageId;

    private bool _notifyIconCreated;

    public enum DWM_WINDOW_CORNER_PREFERENCE
    {
        DWMWCP_DEFAULT = 0,
        DWMWCP_DONOTROUND = 1,
        DWMWCP_ROUND = 2,
        DWMWCP_ROUNDSMALL = 3
    }

    // Properties

    public Guid Id
    {
        get; private set;
    }

    private bool _IsVisible;
    public bool IsVisible
    {
        get => _IsVisible;
        private set
        {
            if (_IsVisible != value)
            {
                _IsVisible = value;

                if (!value)
                    DeleteNotifyIcon();
                else
                    CreateOrModifyNotifyIcon();
            }
        }
    }

    private string _Tooltip;
    public string Tooltip
    {
        get => _Tooltip;
        set
        {
            if (_Tooltip != value)
            {
                _Tooltip = value;

                CreateOrModifyNotifyIcon();
            }
        }
    }

    private System.Drawing.Icon _Icon;
    public System.Drawing.Icon Icon
    {
        get => _Icon;
        set
        {
            if (_Icon != value)
            {
                _Icon = value;

                CreateOrModifyNotifyIcon();
            }
        }
    }

    // Constructor

    public SystemTrayIcon()
    {
        _Icon = new System.Drawing.Icon(PathApp + @"Assets\WindowIcon.ico");
        _Tooltip = "MoreFlyout";
        _taskbarRestartMessageId = PInvoke.RegisterWindowMessage("TaskbarCreated");

        Id = _trayIconGuid;
        _IconWindow = new SystemTrayIconWindow(this);

        CreateOrModifyNotifyIcon();
    }

    // Public Methods

    public SystemTrayIcon Show()
    {
        IsVisible = true;

        return this;
    }

    public SystemTrayIcon Hide()
    {
        IsVisible = false;

        return this;
    }

    // Private Methods

    private void CreateOrModifyNotifyIcon()
    {
        if (IsVisible)
        {
            NOTIFYICONDATAW lpData = default;

            lpData.cbSize = (uint)Marshal.SizeOf(typeof(NOTIFYICONDATAW));
            lpData.hWnd = _IconWindow.WindowHandle;
            lpData.uCallbackMessage = WM_FILES_UNIQUE_MESSAGE;
            lpData.hIcon = (Icon != null) ? new HICON(Icon.Handle) : default;
            lpData.guidItem = Id;
            lpData.uFlags = NOTIFY_ICON_DATA_FLAGS.NIF_MESSAGE | NOTIFY_ICON_DATA_FLAGS.NIF_ICON | NOTIFY_ICON_DATA_FLAGS.NIF_TIP | NOTIFY_ICON_DATA_FLAGS.NIF_GUID | NOTIFY_ICON_DATA_FLAGS.NIF_SHOWTIP;
            lpData.szTip = _Tooltip ?? string.Empty;

            if (!_notifyIconCreated)
            {
                // Delete the existing icon
                PInvoke.Shell_NotifyIcon(NOTIFY_ICON_MESSAGE.NIM_DELETE, in lpData);

                _notifyIconCreated = true;

                // Add a new icon
                PInvoke.Shell_NotifyIcon(NOTIFY_ICON_MESSAGE.NIM_ADD, in lpData);

                lpData.Anonymous.uVersion = 4u;

                // Set the icon handler version
                // NOTE: Do not omit this code. If you remove, the icon won't be shown.
                PInvoke.Shell_NotifyIcon(NOTIFY_ICON_MESSAGE.NIM_SETVERSION, in lpData);
            }
            else
            {
                // Modify the existing icon
                PInvoke.Shell_NotifyIcon(NOTIFY_ICON_MESSAGE.NIM_MODIFY, in lpData);
            }
        }
    }

    private void DeleteNotifyIcon()
    {
        if (_notifyIconCreated)
        {
            _notifyIconCreated = false;

            NOTIFYICONDATAW lpData = default;

            lpData.cbSize = (uint)Marshal.SizeOf(typeof(NOTIFYICONDATAW));
            lpData.hWnd = _IconWindow.WindowHandle;
            lpData.guidItem = Id;
            lpData.uFlags = NOTIFY_ICON_DATA_FLAGS.NIF_MESSAGE | NOTIFY_ICON_DATA_FLAGS.NIF_ICON | NOTIFY_ICON_DATA_FLAGS.NIF_TIP | NOTIFY_ICON_DATA_FLAGS.NIF_GUID | NOTIFY_ICON_DATA_FLAGS.NIF_SHOWTIP;

            // Delete the existing icon
            PInvoke.Shell_NotifyIcon(NOTIFY_ICON_MESSAGE.NIM_DELETE, in lpData);
        }
    }


    internal LRESULT WindowProc(HWND hWnd, uint uMsg, WPARAM wParam, LPARAM lParam)
    {
        switch (uMsg)
        {
            case WM_FILES_UNIQUE_MESSAGE:
                {
                    if ((uint)(lParam.Value & 0xFFFF) == PInvoke.WM_LBUTTONUP | (uint)(lParam.Value & 0xFFFF) == PInvoke.WM_RBUTTONUP)
                    {
                        Point mousePos;
                        PInvoke.GetCursorPos(out mousePos);
                        trayIconContextMenu = new TrayFlyoutContextMenu();
                        trayIconContextMenu.contextMenuWindow.Activate();
                        trayIconContextMenu.contextMenuWindow.AppWindow.MoveAndResize(new Windows.Graphics.RectInt32(mousePos.X, mousePos.Y - 50, 0, 0));
                        trayIconContextMenu.contextMenuWindow.SetForegroundWindow();
                    }

                    break;
                }
            case PInvoke.WM_DESTROY:
                {
                    DeleteNotifyIcon();

                    break;
                }
            default:
                {
                    if (uMsg == _taskbarRestartMessageId)
                    {
                        DeleteNotifyIcon();
                        CreateOrModifyNotifyIcon();
                    }

                    return PInvoke.DefWindowProc(hWnd, uMsg, wParam, lParam);
                }
        }
        return default;
    }

    public void Dispose()
    {
        _IconWindow.Dispose();
    }
}