using MoreFlyout.Config;
using MoreFlyout.Server.Utils;
using MoreFlyout.Server.ViewModels;
using Windows.Win32;
using Windows.Win32.UI.Input.KeyboardAndMouse;

namespace MoreFlyout.Server.Views;

public sealed partial class KeyIndicatorFlyout : UserControl
{
    public KeyIndicatorFlyoutViewModel ViewModel { get; set; }

    // Define key state
    private static bool _NumKeyState;
    private static bool _CapsKeyState;

    // DispatcherTimer to instead Timer
    private readonly DispatcherTimer _hiddenTimer;

    public KeyIndicatorFlyout()
    {
        ViewModel = new KeyIndicatorFlyoutViewModel();
        InitializeComponent();

        // Set DispatcherTimer to control the window will disappear after a certain period of time
        var timeoutSeconds = ConfigManager.Instance.KeyIndicatorFlyoutSettings.TimeoutHiding;
        _hiddenTimer = new DispatcherTimer { Interval = new TimeSpan(0, 0, 0, 0, timeoutSeconds) };
        _hiddenTimer.Tick += OnTimerTick;

        // Get the key state
        _NumKeyState = (PInvoke.GetKeyState((int)VIRTUAL_KEY.VK_NUMLOCK) & 1) == 1;
        _CapsKeyState = (PInvoke.GetKeyState((int)VIRTUAL_KEY.VK_CAPITAL) & 1) == 1;
    }

    private void OnTimerTick(object? sender, object e)
    {
        _hiddenTimer.Stop();
        App.FlyoutControl?.Hide();
    }

    public void InitializeFlyout(int vkCode)
    {
        if (App.FlyoutControl is null)
        {
            return;
        }

        var currentState = false;

        if (vkCode == (int)VIRTUAL_KEY.VK_NUMLOCK)
        {
            _NumKeyState = (PInvoke.GetKeyState((int)VIRTUAL_KEY.VK_NUMLOCK) & 1) == 1;
            currentState = _NumKeyState;
        }
        else if (vkCode == (int)VIRTUAL_KEY.VK_CAPITAL)
        {
            _CapsKeyState = (PInvoke.GetKeyState((int)VIRTUAL_KEY.VK_CAPITAL) & 1) == 1;
            currentState = _CapsKeyState;
        }

        ViewModel.Update(vkCode, currentState);

        var flyoutEnabled = !ConfigManager.Instance.ServiceSettings.GameMode || !Screen.IsFullScreenActive();
        if (!App.FlyoutControl.IsOpen && flyoutEnabled)
        {
            App.FlyoutControl.Show();
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
        _hiddenTimer.Stop();
    }

    private void KeyIndicatorGrid_PointerExited(object sender, Microsoft.UI.Xaml.Input.PointerRoutedEventArgs e)
    {
        _hiddenTimer.Start();
    }
}
