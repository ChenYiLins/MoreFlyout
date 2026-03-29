using MoreFlyout.Config;
using MoreFlyout.Server.Helpers;
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

    // Timer helper for auto-hide functionality
    private readonly FlyoutTimerHelper _timerHelper;

    public KeyIndicatorFlyout()
    {
        ViewModel = new KeyIndicatorFlyoutViewModel();
        InitializeComponent();

        // Set up timer to control the window will disappear after a certain period of time
        var timeoutSeconds = ConfigManager.Instance.KeyIndicatorFlyoutSettings.TimeoutHiding;
        _timerHelper = new FlyoutTimerHelper(timeoutSeconds, () => App.FlyoutControl?.Hide());

        // Get the key state
        _NumKeyState = (PInvoke.GetKeyState((int)VIRTUAL_KEY.VK_NUMLOCK) & 1) == 1;
        _CapsKeyState = (PInvoke.GetKeyState((int)VIRTUAL_KEY.VK_CAPITAL) & 1) == 1;
    }

    public void InitializeFlyout(int vkCode)
    {
        if (App.FlyoutControl is null)
        {
            return;
        }

        if (App.FlyoutControl.IsOpen)
        {
            _timerHelper.Stop();
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

        bool flyoutEnabled = !ConfigManager.Instance.ServiceSettings.GameMode || !Screen.IsFullScreenActive();
        if (!App.FlyoutControl.IsOpen && flyoutEnabled)
        {
            App.FlyoutControl.Show();
        }

        _timerHelper.Start();
    }

    private void KeyIndicatorGrid_PointerEntered(object sender, Microsoft.UI.Xaml.Input.PointerRoutedEventArgs e)
    {
        _timerHelper.Stop();
    }

    private void KeyIndicatorGrid_PointerExited(object sender, Microsoft.UI.Xaml.Input.PointerRoutedEventArgs e)
    {
        _timerHelper.Start();
    }
}
