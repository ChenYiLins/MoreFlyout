using System.Diagnostics;
using System.Runtime.InteropServices;
using Microsoft.UI.Composition;
using Microsoft.UI.Composition.SystemBackdrops;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using MoreFlyout.Helpers;
using MoreFlyout.ViewModels;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.UI.WindowsAndMessaging;

namespace MoreFlyout.Views;

public sealed partial class FlyoutPage : Page
{
    private readonly Microsoft.UI.Dispatching.DispatcherQueue dispatcherQueue;
    private static System.Timers.Timer? aTimer;
    private UnhookWindowsHookExSafeHandle HookID;

    private const int WM_KEYDOWN = 0x0100;
    private const int VK_NUMLOCK = 0x90;
    private const int VK_CAPSLOCK = 0x14;

    private static bool numKeyState = false;
    private static bool capsKeyState = false;

    public FlyoutViewModel ViewModel
    {
        get;
    }

    public FlyoutPage()
    {
        ViewModel = App.GetService<FlyoutViewModel>();
        InitializeComponent();

        dispatcherQueue = Microsoft.UI.Dispatching.DispatcherQueue.GetForCurrentThread();

        numKeyState = (PInvoke.GetKeyState(VK_NUMLOCK) & 1) == 1;
        capsKeyState = (PInvoke.GetKeyState(VK_CAPSLOCK) & 1) == 1;

        HookID = PInvoke.SetWindowsHookEx(WINDOWS_HOOK_ID.WH_KEYBOARD_LL, HookCallback, null, 0);

        App.FlyoutWindow.Closed += FlyoutWindowClosed;
    }

    private void FlyoutWindowClosed(object sender, WindowEventArgs args) => HookID.Close();

    private LRESULT HookCallback(int nCode, WPARAM wParam, LPARAM lParam)
    {
        if (nCode >= 0 && wParam == WM_KEYDOWN)
        {
            var vkCode = Marshal.ReadInt32(lParam);
            if (vkCode == VK_NUMLOCK)
            {
                StatusTextBlock.Text = numKeyState ? "StatusWords_NumUnlock".GetLocalized() : "StatusWords_NumLock".GetLocalized();
                StatusFontIcon.Glyph = numKeyState ? "\uE785" : "\uE72E";
                numKeyState = !numKeyState;
            }
            else if (vkCode == VK_CAPSLOCK)
            {
                StatusTextBlock.Text = capsKeyState ? "StatusWords_CapsUnlock".GetLocalized() : "StatusWords_CapsLock".GetLocalized();
                StatusFontIcon.Glyph = capsKeyState ? "\uE785" : "\uE72E";
                capsKeyState = !capsKeyState;
            }

            if(FlyoutWindowContextFlyout.IsOpen == false) FlyoutWindowContextFlyout.ShowAt(this);
        }
        return PInvoke.CallNextHookEx(null, nCode, wParam, lParam);
    }

    private void SetTimer()
    {
        if (aTimer == null)
        {
            FlyoutWindowContextFlyout.ShowAt(this);
            aTimer = new System.Timers.Timer(2800);
            aTimer.AutoReset = false;
            aTimer.Elapsed += (sender, e) =>
            {
                dispatcherQueue.TryEnqueue(() =>
                {
                    if (FlyoutWindowContextFlyout.IsOpen) FlyoutWindowContextFlyout.Hide();
                });
            };
        }
        else
        {
            aTimer.Stop();
            if (FlyoutWindowContextFlyout.IsOpen == false) FlyoutWindowContextFlyout.ShowAt(this);
        }
        aTimer.Start();
    }
}

public class AcrylicSystemBackdrop : Microsoft.UI.Xaml.Media.SystemBackdrop
{
    DesktopAcrylicController? acrylicController;

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
        SystemBackdropConfiguration defaultConfig = GetDefaultSystemBackdropConfiguration(connectedTarget, xamlRoot);
        defaultConfig.IsInputActive = true;
        defaultConfig.Theme = SystemBackdropTheme.Dark;
        acrylicController.SetSystemBackdropConfiguration(defaultConfig);
        // Add target.
        acrylicController.AddSystemBackdropTarget(connectedTarget);
    }

    protected override void OnTargetDisconnected(ICompositionSupportsSystemBackdrop disconnectedTarget)
    {
        base.OnTargetDisconnected(disconnectedTarget);

        if (acrylicController != null) acrylicController.RemoveSystemBackdropTarget(disconnectedTarget);
        acrylicController = null;
    }
}
