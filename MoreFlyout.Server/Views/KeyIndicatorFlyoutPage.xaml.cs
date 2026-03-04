using MoreFlyout.Config;
using MoreFlyout.Server.Helpers;
using MoreFlyout.Server.Utils;
using MoreFlyout.Server.ViewModels;
using Windows.Win32;
using Windows.Win32.UI.Input.KeyboardAndMouse;
using System.Threading.Tasks;
using Microsoft.UI.Xaml.Media.Animation;

namespace MoreFlyout.Server.Views;

public sealed partial class KeyIndicatorFlyoutPage : Page
{
    public KeyIndicatorFlyoutViewModel ViewModel { get; set; }

    // Define key state
    private static bool _NumKeyState;
    private static bool _CapsKeyState;

    // DispatcherTimer to instead Timer
    private readonly DispatcherTimer _hiddenTimer;

    public KeyIndicatorFlyoutPage()
    {
        ViewModel = new KeyIndicatorFlyoutViewModel();
        InitializeComponent();
        RequestedTheme = SystemTheme.GetCurrentSystemTheme();

        // Set DispatcherTimer to control the window will disappear after a certain period of time
        var timeoutSeconds = ConfigManager.Instance.KeyIndicatorFlyout.TimeoutHiding;
        _hiddenTimer = new DispatcherTimer { Interval = new TimeSpan(0, 0, 0, 0, timeoutSeconds) };
        _hiddenTimer.Tick += OnTimerTick;

        // Get the key state
        _NumKeyState = (PInvoke.GetKeyState((int)VIRTUAL_KEY.VK_NUMLOCK) & 1) == 1;
        _CapsKeyState = (PInvoke.GetKeyState((int)VIRTUAL_KEY.VK_CAPITAL) & 1) == 1;
    }

    private void OnTimerTick(object? sender, object e)
    {
        _hiddenTimer.Stop();
        AnimatedPopupHelpers.HidePopupWithAnimation(PagePopup, FlyoutBorder);
    }

    public void InitializeFlyout(int vkCode)
    {
        switch (vkCode)
        {
            case (int)VIRTUAL_KEY.VK_NUMLOCK:
                ModeFontIcon.Glyph = "\uF146";
                StatusTextBlock.Text = _NumKeyState ? "StatusWords_NumUnlock".GetLocalized() : "StatusWords_NumLock".GetLocalized();
                StatusFontIcon.Glyph = _NumKeyState ? "\uE785" : "\uE72E";
                if (_NumKeyState)
                {
                    VisualStateManager.GoToState(IconHyperlinkButton, "Normal", true);
                }
                else
                {
                    DispatcherQueue.TryEnqueue(
                        Microsoft.UI.Dispatching.DispatcherQueuePriority.Low,
                        () =>
                        {
                            VisualStateManager.GoToState(IconHyperlinkButton, "PointerOver", true);
                        }
                    );
                }
                _NumKeyState = !_NumKeyState;
                break;
            case (int)VIRTUAL_KEY.VK_CAPITAL:
                ModeFontIcon.Glyph = "\uE97E";
                StatusTextBlock.Text = _CapsKeyState ? "StatusWords_CapsUnlock".GetLocalized() : "StatusWords_CapsLock".GetLocalized();
                StatusFontIcon.Glyph = _CapsKeyState ? "\uE785" : "\uE72E";
                if (_CapsKeyState)
                {
                    IconHyperlinkButton.Style = (Style)Application.Current.Resources["HyperlinkButtonStyle"];
                }
                else
                {
                    IconHyperlinkButton.Style = (Style)Application.Current.Resources["HyperlinkButtonStylePointerOver"];
                }
                _CapsKeyState = !_CapsKeyState;
                break;
        }

        bool flyoutEnabled = !ConfigManager.Instance.ServiceSettings.GameMode || !Screen.IsFullScreenActive();
        if (!PagePopup.IsOpen && flyoutEnabled)
        {
            AnimatedPopupHelpers.OpenPopupWithAnimation(PagePopup, FlyoutBorder);
        }

        RunTimer();
    }

    private void RunTimer()
    {
        _hiddenTimer.Stop();
        _hiddenTimer.Start();
    }

    private void KeyIndicatorGrid_PointerEntered(object sender, Microsoft.UI.Xaml.Input.PointerRoutedEventArgs e)
    {
        _hiddenTimer?.Stop();
    }

    private void KeyIndicatorGrid_PointerExited(object sender, Microsoft.UI.Xaml.Input.PointerRoutedEventArgs e)
    {
        _hiddenTimer?.Start();
    }
}
