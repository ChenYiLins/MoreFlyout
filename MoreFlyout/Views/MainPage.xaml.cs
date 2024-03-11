using System.Runtime.InteropServices;
using Microsoft.UI.Xaml.Controls;
using MoreFlyout.Helpers;
using MoreFlyout.ViewModels;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.UI.WindowsAndMessaging;

namespace MoreFlyout.Views;

public sealed partial class MainPage : Page
{
    private System.Timers.Timer? Timer;

    private const int WM_KEYDOWN = 0x0100;
    private const int VK_NUMLOCK = 0x90;
    private const int VK_CAPSLOCK = 0x14;

    private static SafeHandle? _HookID;
    private static bool numKeyState = false;
    private static bool capsKeyState = false;

    public MainViewModel ViewModel
    {
        get;
    }

    public MainPage()
    {
        ViewModel = App.GetService<MainViewModel>();
        InitializeComponent();

        if ((PInvoke.GetKeyState(VK_NUMLOCK) & 1) == 1)
        {
            numKeyState = true;
        }
        else
        {
            numKeyState = false;
        }

        if ((PInvoke.GetKeyState(VK_CAPSLOCK) & 1) == 1)
        {
            capsKeyState = true;
        }
        else
        {
            capsKeyState = false;
        }

        _HookID = PInvoke.SetWindowsHookEx(WINDOWS_HOOK_ID.WH_KEYBOARD_LL, HookCallback, null, 0);

        Loaded += MainPage_Loaded;
    }

    private void MainPage_Loaded(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        App.MainWindow.IsAlwaysOnTop = true;
        App.MainWindow.Hide();
    }

    private LRESULT HookCallback(int nCode, WPARAM wParam, LPARAM lParam)
    {
        if (nCode >= 0 && wParam == WM_KEYDOWN)
        {
            var vkCode = Marshal.ReadInt32(lParam);
            if (vkCode == VK_NUMLOCK)
            {
                if (numKeyState == true)
                {
                    StatusFontIcon.Glyph = "\uE785";
                    StatusTextBlock.Text = "StatusWords_NumUnlock".GetLocalized();
                    App.MainWindow.Show();
                    StartTimer();
                    numKeyState = false;
                }
                else
                {
                    StatusFontIcon.Glyph = "\uE72E";
                    StatusTextBlock.Text = "StatusWords_NumLock".GetLocalized();
                    App.MainWindow.Show();
                    StartTimer();
                    numKeyState = true;
                }
            }
            else if (vkCode == VK_CAPSLOCK)
            {
                if (capsKeyState == true)
                {
                    StatusFontIcon.Glyph = "\uE785";
                    StatusTextBlock.Text = "StatusWords_CapsUnlock".GetLocalized();
                    App.MainWindow.Show();
                    StartTimer();
                    capsKeyState = false;
                }
                else
                {
                    StatusFontIcon.Glyph = "\uE72E";
                    StatusTextBlock.Text = "StatusWords_CapsLock".GetLocalized();
                    App.MainWindow.Show();
                    StartTimer();
                    capsKeyState = true;
                }
            }
        }
        return PInvoke.CallNextHookEx(_HookID, nCode, wParam, lParam);
    }

    private void StartTimer()
    {
        if (Timer == null)
        {
            Timer = new System.Timers.Timer(2800);
            Timer.Elapsed += (sender, e) =>
            {
                App.MainWindow.Hide();
            };
        }
        else
        {
            Timer.Stop();
        }

        Timer.Start();
    }
}
