using MoreFlyout.Config;
using MoreFlyout.Server.Helpers;
using MoreFlyout.Server.Utils;
using MoreFlyout.Server.ViewModels;

namespace MoreFlyout.Server.Views;

public sealed partial class MediaFlyout : UserControl
{
    public MediaFlyoutViewModel ViewModel { get; set; }

    // Timer helper for auto-hide functionality
    private readonly FlyoutTimerHelper _timerHelper;

    public MediaFlyout()
    {
        ViewModel = new MediaFlyoutViewModel();
        InitializeComponent();

        // Set up timer to control the window will disappear after a certain period of time
        var timeoutSeconds = ConfigManager.Instance.MediaFlyoutSettings.TimeoutHiding;
        _timerHelper = new FlyoutTimerHelper(timeoutSeconds, () =>
        {
            if (App.FlyoutControl?.IsOpen == true)
            {
                App.FlyoutControl.Hide();
            }
        });
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

        bool flyoutEnabled = !ConfigManager.Instance.ServiceSettings.GameMode || !Screen.IsFullScreenActive();
        if (!App.FlyoutControl.IsOpen && flyoutEnabled)
        {
            App.FlyoutControl.Show();
        }

        _timerHelper.Start();
    }

    private void MediaGrid_PointerEntered(object sender, Microsoft.UI.Xaml.Input.PointerRoutedEventArgs e)
    {
        _timerHelper.Stop();
    }

    private void MediaGrid_PointerExited(object sender, Microsoft.UI.Xaml.Input.PointerRoutedEventArgs e)
    {
        _timerHelper.Start();
    }
}
