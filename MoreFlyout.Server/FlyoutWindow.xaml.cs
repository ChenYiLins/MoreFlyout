using System.Diagnostics;
using System.Runtime.InteropServices;
using Microsoft.UI.Dispatching;
using MoreFlyout.Config;
using MoreFlyout.Server.UserControls.AnimationFlyout;
using MoreFlyout.Server.Views;
using Windows.UI;
using Windows.UI.ViewManagement;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.UI.Controls;
using Windows.Win32.UI.Input.KeyboardAndMouse;
using Windows.Win32.UI.WindowsAndMessaging;

namespace MoreFlyout.Server;

public sealed partial class FlyoutWindow : WindowEx
{
    private static readonly Logger _Logger = LogManager.GetCurrentClassLogger();

    private static readonly KeyIndicatorFlyoutPage _KeyIndicatorFlyoutPage = new();
    private static readonly MediaFlyoutPage _MediaFlyoutPage = new();
    private static readonly DarkModeFlyoutPage _DarkModeFlyoutPage = new();

    private UISettings? _settings;

    // This static assignment will ensure GC doesn't move the procedure around
    private static readonly HOOKPROC _KeyboardHook = KeyboardCallback;
    private static readonly HOOKPROC _CallWndProcHook = CallWndProcCallback;
    private static UnhookWindowsHookExSafeHandle? _KeyboardHookId;
    private static UnhookWindowsHookExSafeHandle? _CallWndProcHookId;

    public FlyoutWindow()
    {
        InitializeComponent();

        InitializeWindow();

        InitializeInstanceModules();

        // Start watching for config changes
        ConfigManager.StartWatching();
        ConfigManager.ConfigChanged += OnConfigChanged;

        // Cleanup when window is closed
        AppDomain.CurrentDomain.ProcessExit += OnProcessExit;
    }

    static FlyoutWindow()
    {
        InitializeStaticModules();
    }

    private void OnProcessExit(object? sender, EventArgs e)
    {
        ConfigManager.ConfigChanged -= OnConfigChanged;
        ConfigManager.StopWatching();
        TeardownInstanceModules();
        TeardownStaticModules();
    }

    private void OnConfigChanged(object? sender, EventArgs e)
    {
        DispatcherQueue.TryEnqueue(() =>
        {
            TeardownInstanceModules();
            InitializeInstanceModules();

            TeardownStaticModules();
            InitializeStaticModules();
        });

        _Logger.Info("The program exits and all modules are removed");
    }

    private void InitializeWindow()
    {
        IsShownInSwitchers = false;
        IsResizable = false;

        var hWnd = (HWND)WinRT.Interop.WindowNative.GetWindowHandle(this);
        _ = PInvoke.SetWindowLong(
            hWnd,
            WINDOW_LONG_PTR_INDEX.GWL_EXSTYLE,
            PInvoke.GetWindowLong(hWnd, WINDOW_LONG_PTR_INDEX.GWL_EXSTYLE)
                | (int)WINDOW_EX_STYLE.WS_EX_LAYERED
                | (int)WINDOW_EX_STYLE.WS_EX_NOACTIVATE
                | (int)WINDOW_EX_STYLE.WS_EX_TOPMOST
                | (int)WINDOW_EX_STYLE.WS_EX_TOOLWINDOW
        );
        _ = PInvoke.SetWindowLong(
            hWnd,
            WINDOW_LONG_PTR_INDEX.GWL_STYLE,
            PInvoke.GetWindowLong(hWnd, WINDOW_LONG_PTR_INDEX.GWL_STYLE) | unchecked((int)WINDOW_STYLE.WS_POPUP) | (int)WINDOW_STYLE.WS_CLIPCHILDREN
        );
        //this.SetWindowOpacity(0);
        PInvoke.SetLayeredWindowAttributes(hWnd, (COLORREF)0, 0, LAYERED_WINDOW_ATTRIBUTES_FLAGS.LWA_ALPHA);
        var margins = new MARGINS
        {
            cxLeftWidth = -1,
            cxRightWidth = -1,
            cyBottomHeight = -1,
            cyTopHeight = -1,
        };
        PInvoke.DwmExtendFrameIntoClientArea(hWnd, in margins);

        var screenWidth = PInvoke.GetSystemMetrics(SYSTEM_METRICS_INDEX.SM_CXSCREEN);
        var screenHeight = PInvoke.GetSystemMetrics(SYSTEM_METRICS_INDEX.SM_CYSCREEN);
        var dpiForWindow = PInvoke.GetDpiForWindow(hWnd);
        //var windowRatio = dpiForWindow / 96.0;
        AppWindow.MoveAndResize(new Windows.Graphics.RectInt32(0, 0, screenWidth, screenHeight));
        //AppWindow.Resize(new Windows.Graphics.SizeInt32(screenWidth, screenHeight));
        //AppWindow.Move(new Windows.Graphics.PointInt32(screenWidth / 2 - (int)(195 / 2 * windowRatio), screenHeight - (int)(86 * windowRatio)));
    }

    private void InitializeInstanceModules()
    {
        if (ConfigManager.Instance.DarkModeFlyout.IsEnabled)
        {
            _settings = new UISettings();
            _settings.ColorValuesChanged += (s, e) =>
            {
                if (App.FlyoutWindow is null)
                {
                    return;
                }

                if (!App.FlyoutWindow.IsAlwaysOnTop)
                {
                    App.FlyoutWindow.IsAlwaysOnTop = true;
                }

                var darkModeEnabled = _settings.GetColorValue(UIColorType.Foreground) == Color.FromArgb(0xFF, 0xFF, 0xFF, 0xFF);

                App.FlyoutWindow.DispatcherQueue.TryEnqueue(() =>
                {
                    App.FlyoutWindow.Content = _DarkModeFlyoutPage;
                    App.FlyoutWindow.DispatcherQueue.TryEnqueue(Microsoft.UI.Dispatching.DispatcherQueuePriority.Low, () => _DarkModeFlyoutPage.InitializeFlyout(darkModeEnabled));
                });
            };

            _Logger.Info("Auto Dark Mode flyout is enabled");
        }
        else
        {
            _Logger.Info("Auto Dark Mode flyout is not enabled");
        }
    }

    private void TeardownInstanceModules()
    {
        if (_settings is not null)
        {
            _settings = null;
        }
    }

    private static void InitializeStaticModules()
    {
        // KeyIndicatorFlyout and MediaFlyout
        if (ConfigManager.Instance.KeyIndicatorFlyout.IsEnabled || ConfigManager.Instance.MediaFlyout.IsEnabled)
        {
            // Hook here (expected to be done in UI thread in our case, it facilitates everything)
            _KeyboardHookId = PInvoke.SetWindowsHookEx(WINDOWS_HOOK_ID.WH_KEYBOARD_LL, _KeyboardHook, null, 0);
            var hModule = PInvoke.GetModuleHandle(Process.GetCurrentProcess().MainModule!.ModuleName);
            _CallWndProcHookId = PInvoke.SetWindowsHookEx(WINDOWS_HOOK_ID.WH_CALLWNDPROC, _CallWndProcHook, hModule, PInvoke.GetCurrentThreadId());

            _Logger.Info("Key Indicator or Media flyouts is enabled, install hooks");
        }
        else
        {
            _Logger.Info("Key Indicator and Media flyouts is not enabled");
        }
    }

    private static void TeardownStaticModules()
    {
        // Unhook on exit (more or less useless)
        if (_KeyboardHookId is not null && !_KeyboardHookId.IsClosed)
        {
            _KeyboardHookId.Close();
            _KeyboardHookId = null;
        }

        if (_CallWndProcHookId is not null && !_CallWndProcHookId.IsClosed)
        {
            _CallWndProcHookId.Close();
            _CallWndProcHookId = null;
        }
    }

    // The hook function must be static
    private static LRESULT KeyboardCallback(int nCode, WPARAM wParam, LPARAM lParam)
    {
        // Lucky us WH_KEYBOARD_LL calls back on initial hooking thread, ie: the UI thread
        // so no need for Dispatcher mambo jumbo
        KeyboardHookCallbackAsync(nCode, wParam, lParam);
        return PInvoke.CallNextHookEx(_KeyboardHookId, nCode, wParam, lParam);
    }

    private static LRESULT CallWndProcCallback(int nCode, WPARAM wParam, LPARAM lParam)
    {
        KeyboardHookCallbackAsync(nCode, wParam, lParam);
        CallWndProcHookCallback(nCode, wParam, lParam);
        return PInvoke.CallNextHookEx(_CallWndProcHookId, nCode, wParam, lParam);
    }

    private static async void KeyboardHookCallbackAsync(int nCode, WPARAM wParam, LPARAM lParam)
    {
        if (nCode >= 0 && wParam == PInvoke.WM_KEYDOWN)
        {
            var vkCode = Marshal.ReadInt32(lParam);

            if (App.FlyoutWindow is null)
            {
                return;
            }

            if (!App.FlyoutWindow.IsAlwaysOnTop)
            {
                App.FlyoutWindow.IsAlwaysOnTop = true;
            }

            if (vkCode == (int)VIRTUAL_KEY.VK_NUMLOCK || vkCode == (int)VIRTUAL_KEY.VK_CAPITAL && ConfigManager.Instance.KeyIndicatorFlyout.IsEnabled)
            {
                _KeyIndicatorFlyoutPage.InitializeFlyout(vkCode);
            }
            else if (
                vkCode == (int)VIRTUAL_KEY.VK_MEDIA_PLAY_PAUSE
                || vkCode == (int)VIRTUAL_KEY.VK_MEDIA_NEXT_TRACK
                || vkCode == (int)VIRTUAL_KEY.VK_MEDIA_PREV_TRACK
                || vkCode == (int)VIRTUAL_KEY.VK_MEDIA_STOP && ConfigManager.Instance.MediaFlyout.IsEnabled
            )
            {
                App.FlyoutWindow.Content = _MediaFlyoutPage;
                App.FlyoutWindow.DispatcherQueue.TryEnqueue(Microsoft.UI.Dispatching.DispatcherQueuePriority.Low, () => _MediaFlyoutPage.InitializeFlyout(vkCode));
            }
        }
    }

    private static void CallWndProcHookCallback(int nCode, WPARAM wParam, LPARAM lParam)
    {
        if (nCode >= 0)
        {
            var msg = Marshal.PtrToStructure<CWPSTRUCT>(lParam);
            if (msg.message == PInvoke.RegisterWindowMessage("TaskbarCreated"))
            {
                if (App.FlyoutWindow is null)
                {
                    return;
                }
                App.FlyoutWindow.IsShownInSwitchers = true;
                App.FlyoutWindow.IsShownInSwitchers = false;
            }
        }
    }

    private void Flyout_Closed(object sender, object e)
    {
        this.Hide();
    }
}
