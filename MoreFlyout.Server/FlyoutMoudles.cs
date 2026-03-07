using System.Diagnostics;
using System.Runtime.InteropServices;
using MoreFlyout.Config;
using MoreFlyout.Server.Views;
using Windows.UI;
using Windows.UI.ViewManagement;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.UI.Input.KeyboardAndMouse;
using Windows.Win32.UI.WindowsAndMessaging;

namespace MoreFlyout.Server;

public class FlyoutMoudles
{
    private static readonly Logger _Logger = LogManager.GetCurrentClassLogger();

    // This static assignment will ensure GC doesn't move the procedure around
    private static readonly HOOKPROC _KeyboardHook = KeyboardCallback;
    private static readonly HOOKPROC _CallWndProcHook = CallWndProcCallback;
    private static UnhookWindowsHookExSafeHandle? _KeyboardHookId;
    private static UnhookWindowsHookExSafeHandle? _CallWndProcHookId;

    private UISettings? _settings;

    private static readonly KeyIndicatorFlyout _KeyIndicatorFlyout = new();
    private static readonly MediaFlyout _MediaFlyout = new();
    private static readonly DarkModeFlyout _DarkModeFlyout = new();

    public FlyoutMoudles()
    {
        InitializeInstanceModules();

        // Start watching for config changes
        ConfigManager.StartWatching();
        ConfigManager.ConfigChanged += OnConfigChanged;
    }

    static FlyoutMoudles()
    {
        InitializeStaticModules();
    }

    private void InitializeInstanceModules()
    {
        if (ConfigManager.Instance.DarkModeFlyoutSettings.IsEnabled)
        {
            _settings = new UISettings();
            _settings.ColorValuesChanged += (s, e) =>
            {
                if (App.FlyoutControl is null)
                {
                    return;
                }

                var darkModeEnabled = _settings.GetColorValue(UIColorType.Foreground) == Color.FromArgb(0xFF, 0xFF, 0xFF, 0xFF);

                App.FlyoutControl.DispatcherQueue.TryEnqueue(()=>
                {
                    App.FlyoutControl.RootContent = _DarkModeFlyout;
                    _DarkModeFlyout.InitializeFlyout(darkModeEnabled);
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
        if (ConfigManager.Instance.KeyIndicatorFlyoutSettings.IsEnabled || ConfigManager.Instance.MediaFlyoutSettings.IsEnabled)
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

    private void OnConfigChanged(object? sender, EventArgs e)
    {
        TeardownInstanceModules();
        InitializeInstanceModules();

        TeardownStaticModules();
        InitializeStaticModules();

        _Logger.Info("The program exits and all modules are removed");
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
        return PInvoke.CallNextHookEx(_CallWndProcHookId, nCode, wParam, lParam);
    }

    private static async void KeyboardHookCallbackAsync(int nCode, WPARAM wParam, LPARAM lParam)
    {
        if (nCode >= 0 && wParam == PInvoke.WM_KEYDOWN)
        {
            var vkCode = Marshal.ReadInt32(lParam);

            if (App.FlyoutControl is null)
            {
                return;
            }

            if (vkCode == (int)VIRTUAL_KEY.VK_NUMLOCK || vkCode == (int)VIRTUAL_KEY.VK_CAPITAL && ConfigManager.Instance.KeyIndicatorFlyoutSettings.IsEnabled)
            {
                App.FlyoutControl.RootContent = _KeyIndicatorFlyout;
                _KeyIndicatorFlyout.InitializeFlyout(vkCode);
            }
            else if (
                vkCode == (int)VIRTUAL_KEY.VK_MEDIA_PLAY_PAUSE
                || vkCode == (int)VIRTUAL_KEY.VK_MEDIA_NEXT_TRACK
                || vkCode == (int)VIRTUAL_KEY.VK_MEDIA_PREV_TRACK
                || vkCode == (int)VIRTUAL_KEY.VK_MEDIA_STOP && ConfigManager.Instance.MediaFlyoutSettings.IsEnabled
            )
            {
                App.FlyoutControl.RootContent = _MediaFlyout;
                _MediaFlyout.InitializeFlyout(vkCode);
            }
        }
    }

    public void Dispose()
    {
        ConfigManager.ConfigChanged -= OnConfigChanged;
        ConfigManager.StopWatching();
        TeardownInstanceModules();
        TeardownStaticModules();
    }
}
