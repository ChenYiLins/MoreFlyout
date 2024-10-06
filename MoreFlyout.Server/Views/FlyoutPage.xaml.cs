using System.Diagnostics;
using System.Runtime.InteropServices;
using Microsoft.UI.Composition;
using Microsoft.UI.Composition.SystemBackdrops;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using MoreFlyout.Server.Contracts.Services;
using MoreFlyout.Server.Helpers;
using MoreFlyout.Server.ViewModels;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.UI.WindowsAndMessaging;

namespace MoreFlyout.Server.Views;

public sealed partial class FlyoutPage : Page
{
    // DispatcherTimer to instead Timer
    private readonly DispatcherTimer hiddenTimer;

    // This static assignment will ensure GC doesn't move the procedure around
    private static readonly HOOKPROC _keyboardHook = KeyboardCallback;
    private static readonly HOOKPROC _callWndProcHook = CallWndProcCallback;
    private static readonly UnhookWindowsHookExSafeHandle _keyboardHookId;
    private static readonly UnhookWindowsHookExSafeHandle _callWndProcHookId;

    // Define different key event codes
    private const int WM_KEYDOWN = 0x0100;
    private const int VK_NUMLOCK = 0x90;
    private const int VK_CAPSLOCK = 0x14;

    // Define key state
    private static bool numKeyState = false;
    private static bool capsKeyState = false;

    public FlyoutViewModel ViewModel
    {
        get;
    }

    static FlyoutPage()
    {
        // Hook here (expected to be done in UI thread in our case, it facilitates everything)
        _keyboardHookId = PInvoke.SetWindowsHookEx(WINDOWS_HOOK_ID.WH_KEYBOARD_LL, _keyboardHook, null, 0);
        var hModule = PInvoke.GetModuleHandle(Process.GetCurrentProcess().MainModule!.ModuleName);
        _callWndProcHookId = PInvoke.SetWindowsHookEx(WINDOWS_HOOK_ID.WH_CALLWNDPROC, _callWndProcHook, hModule, PInvoke.GetCurrentThreadId());

        // Unhook on exit (more or less useless)
        AppDomain.CurrentDomain.ProcessExit += (s, e) => { _keyboardHookId.Close(); };
        AppDomain.CurrentDomain.ProcessExit += (s, e) => { _callWndProcHookId.Close(); };
    }

    public FlyoutPage()
    {
        ViewModel = App.GetService<FlyoutViewModel>();
        InitializeComponent();

        // Set DispatcherTimer to control the window will disappear after a certain period of time
        hiddenTimer = new DispatcherTimer
        {
            Interval = new TimeSpan(0, 0, 0, 0, 2800)
        };
        hiddenTimer.Stop();

        // Get the key state
        numKeyState = (PInvoke.GetKeyState(VK_NUMLOCK) & 1) == 1;
        capsKeyState = (PInvoke.GetKeyState(VK_CAPSLOCK) & 1) == 1;

        //Microsoft.Windows.Globalization.ApplicationLanguages.PrimaryLanguageOverride = "en-us";
    }

    // The hook function must be static
    private static LRESULT KeyboardCallback(int nCode, WPARAM wParam, LPARAM lParam)
    {
        // Lucky us WH_KEYBOARD_LL calls back on initial hooking thread, ie: the UI thread
        // so no need for Dispatcher mumbo jumbo

        // Get your navigation service and defer to the instance method if found
        var navigation = App.GetService<INavigationService>();
        if (navigation?.Frame != null)
        {
            if (navigation.Frame.Content is not FlyoutPage)
            {
                navigation.NavigateTo(typeof(FlyoutViewModel).FullName!);
            }

            if (navigation.Frame.Content is FlyoutPage page)
            {
                page.KeyboardHookCallback(nCode, wParam, lParam);
            }
        }
        return PInvoke.CallNextHookEx(_keyboardHookId, nCode, wParam, lParam);
    }

    private static LRESULT CallWndProcCallback(int nCode, WPARAM wParam, LPARAM lParam)
    {
        // Get your navigation service and defer to the instance method if found
        var navigation = App.GetService<INavigationService>();
        if (navigation?.Frame != null)
        {
            if (navigation.Frame.Content is not FlyoutPage)
            {
                navigation.NavigateTo(typeof(FlyoutViewModel).FullName!);
            }

            if (navigation.Frame.Content is FlyoutPage)
            {
                CallWndProcHookCallback(nCode, wParam, lParam);
            }
        }
        return PInvoke.CallNextHookEx(_callWndProcHookId, nCode, wParam, lParam);
    }

    private void KeyboardHookCallback(int nCode, WPARAM wParam, LPARAM lParam)
    {
        if (nCode >= 0 && wParam == WM_KEYDOWN)
        {
            var vkCode = Marshal.ReadInt32(lParam);

            if (App.FlyoutWindow.IsAlwaysOnTop == false) App.FlyoutWindow.IsAlwaysOnTop = true;

            if (vkCode == VK_NUMLOCK || vkCode == VK_CAPSLOCK)
            {
                switch (vkCode)
                {
                    case VK_NUMLOCK:
                        StatusTextBlock.Text = numKeyState ? "StatusWords_NumUnlock".GetLocalized() : "StatusWords_NumLock".GetLocalized();
                        StatusFontIcon.Glyph = numKeyState ? "\uE785" : "\uE72E";
                        numKeyState = !numKeyState;
                        break;
                    case VK_CAPSLOCK:
                        StatusTextBlock.Text = capsKeyState ? "StatusWords_CapsUnlock".GetLocalized() : "StatusWords_CapsLock".GetLocalized();
                        StatusFontIcon.Glyph = capsKeyState ? "\uE785" : "\uE72E";
                        capsKeyState = !capsKeyState;
                        break;
                }
                if (FlyoutPageContextFlyout.IsOpen == false && IsFullScreenActive() == false) FlyoutPageContextFlyout.ShowAt(this);
                RunTimer();
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
                App.FlyoutWindow.IsShownInSwitchers = true;
                App.FlyoutWindow.IsShownInSwitchers = false;
            }
        }
    }

    private void RunTimer()
    {
        if (hiddenTimer.IsEnabled == false)
        {
            hiddenTimer.Start();
            hiddenTimer.Tick += (sender, e) =>
            {
                if (FlyoutPageContextFlyout.IsOpen) FlyoutPageContextFlyout.Hide();
                hiddenTimer.Stop();
            };
        }
        else
        {
            hiddenTimer.Stop();
        }
        hiddenTimer.Start();
    }

    private unsafe bool IsFullScreenActive()
    {
        var isFullScreen = true;
        const int MAX_PATH = 260;
        Span<char> buffer = new char[MAX_PATH + 1];

        var screenWidth = PInvoke.GetSystemMetrics(SYSTEM_METRICS_INDEX.SM_CXSCREEN);
        var screenHeight = PInvoke.GetSystemMetrics(SYSTEM_METRICS_INDEX.SM_CYSCREEN);

        System.Drawing.Point[] screenCorners =
        [
            new(1, screenHeight - 1),
            new(screenWidth - 1, screenHeight - 1)
        ];

        foreach (var corner in screenCorners)
        {
            var hWnd = PInvoke.WindowFromPoint(corner);

            fixed (char* pBuffer = buffer)
            {
                PWSTR pWSTR = pBuffer;
                _ = PInvoke.GetClassName(hWnd, pWSTR, MAX_PATH);
                if (pWSTR.ToString() == "Shell_TrayWnd" || pWSTR.ToString() == "TrayNotifyWnd") isFullScreen = false;
            }
        }

        return isFullScreen;
    }

}

public partial class AcrylicSystemBackdrop : Microsoft.UI.Xaml.Media.SystemBackdrop
{
    private DesktopAcrylicController? acrylicController;

    protected override void OnTargetConnected(ICompositionSupportsSystemBackdrop connectedTarget, XamlRoot xamlRoot)
    {
        // Call the base method to initialize the default configuration object.
        base.OnTargetConnected(connectedTarget, xamlRoot);

        // This example does not support sharing MicaSystemBackdrop instances.
        if (acrylicController is not null)
        {
            throw new Exception("This controller cannot be shared");
        }

        acrylicController = new DesktopAcrylicController();

        // Set configuration.
        var defaultConfig = GetDefaultSystemBackdropConfiguration(connectedTarget, xamlRoot);
        defaultConfig.IsInputActive = true;
        defaultConfig.Theme = SystemBackdropTheme.Dark;
        acrylicController.SetSystemBackdropConfiguration(defaultConfig);

        // Add target.
        acrylicController.AddSystemBackdropTarget(connectedTarget);
    }

    protected override void OnTargetDisconnected(ICompositionSupportsSystemBackdrop disconnectedTarget)
    {
        base.OnTargetDisconnected(disconnectedTarget);

        acrylicController?.RemoveSystemBackdropTarget(disconnectedTarget);
        acrylicController = null;
    }
}
