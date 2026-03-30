using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
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
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    // This static assignment will ensure GC doesn't move the procedure around
    private static readonly HOOKPROC KeyboardHook = KeyboardCallback;
    private static readonly HOOKPROC CallWndProcHook = CallWndProcCallback;
    private static UnhookWindowsHookExSafeHandle? KeyboardHookId;
    private static UnhookWindowsHookExSafeHandle? CallWndProcHookId;

    private UISettings? _settings;

    [AllowNull]
    private static KeyIndicatorFlyout KeyIndicatorFlyout
    {
        get => field ??= new KeyIndicatorFlyout();
        set;
    }

    [AllowNull]
    private static MediaFlyout MediaFlyout
    {
        get => field ??= new MediaFlyout();
        set;
    }

    [AllowNull]
    private static DarkModeFlyout DarkModeFlyout
    {
        get => field ??= new DarkModeFlyout();
        set;
    }

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

                App.FlyoutControl.DispatcherQueue.TryEnqueue(() =>
                {
                    App.FlyoutControl.RootContent = DarkModeFlyout;
                    DarkModeFlyout.InitializeFlyout(darkModeEnabled);
                });
            };

            Logger.Info("Auto Dark Mode flyout is enabled");
        }
        else
        {
            Logger.Info("Auto Dark Mode flyout is not enabled");
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
            KeyboardHookId = PInvoke.SetWindowsHookEx(WINDOWS_HOOK_ID.WH_KEYBOARD_LL, KeyboardHook, null, 0);
            var hModule = PInvoke.GetModuleHandle(Process.GetCurrentProcess().MainModule!.ModuleName);
            CallWndProcHookId = PInvoke.SetWindowsHookEx(WINDOWS_HOOK_ID.WH_CALLWNDPROC, CallWndProcHook, hModule, PInvoke.GetCurrentThreadId());

            Logger.Info("Key Indicator or Media flyouts is enabled, install hooks");
        }
        else
        {
            Logger.Info("Key Indicator and Media flyouts is not enabled");
        }
    }

    private static void TeardownStaticModules()
    {
        // Unhook on exit (more or less useless)
        if (KeyboardHookId is not null && !KeyboardHookId.IsClosed)
        {
            KeyboardHookId.Close();
            KeyboardHookId = null;
        }

        if (CallWndProcHookId is not null && !CallWndProcHookId.IsClosed)
        {
            CallWndProcHookId.Close();
            CallWndProcHookId = null;
        }

        KeyIndicatorFlyout = null;
        MediaFlyout = null;
        DarkModeFlyout = null;
    }

    private void OnConfigChanged(object? sender, EventArgs e)
    {
        TeardownInstanceModules();
        TeardownStaticModules();

        Logger.Info("The program exits and all modules are removed");

        App.FlyoutControl?.DispatcherQueue.TryEnqueue(() =>
        {
            InitializeStaticModules();
            InitializeInstanceModules();
        });
    }

    // The hook function must be static
    private static LRESULT KeyboardCallback(int nCode, WPARAM wParam, LPARAM lParam)
    {
        // Lucky us WH_KEYBOARD_LL calls back on initial hooking thread, ie: the UI thread
        // so no need for Dispatcher mambo jumbo
        KeyboardHookCallback(nCode, wParam, lParam);
        return PInvoke.CallNextHookEx(KeyboardHookId, nCode, wParam, lParam);
    }

    private static LRESULT CallWndProcCallback(int nCode, WPARAM wParam, LPARAM lParam)
    {
        KeyboardHookCallback(nCode, wParam, lParam);
        return PInvoke.CallNextHookEx(CallWndProcHookId, nCode, wParam, lParam);
    }

    private static void KeyboardHookCallback(int nCode, WPARAM wParam, LPARAM lParam)
    {
        if (nCode >= 0 && wParam == PInvoke.WM_KEYDOWN)
        {
            var vkCode = Marshal.ReadInt32(lParam);

            if (App.FlyoutControl is null)
            {
                return;
            }

            if ((vkCode == (int)VIRTUAL_KEY.VK_NUMLOCK || vkCode == (int)VIRTUAL_KEY.VK_CAPITAL) && ConfigManager.Instance.KeyIndicatorFlyoutSettings.IsEnabled)
            {
                App.FlyoutControl.RootContent = KeyIndicatorFlyout;
                KeyIndicatorFlyout.InitializeFlyout(vkCode);
            }
            else if (
                (
                    vkCode == (int)VIRTUAL_KEY.VK_MEDIA_PLAY_PAUSE
                    || vkCode == (int)VIRTUAL_KEY.VK_MEDIA_NEXT_TRACK
                    || vkCode == (int)VIRTUAL_KEY.VK_MEDIA_PREV_TRACK
                    || vkCode == (int)VIRTUAL_KEY.VK_MEDIA_STOP
                ) && ConfigManager.Instance.MediaFlyoutSettings.IsEnabled
            )
            {
                App.FlyoutControl.RootContent = MediaFlyout;
                MediaFlyout.InitializeFlyout(vkCode);
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
