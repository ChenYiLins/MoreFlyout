using MoreFlyout.Config;
using MoreFlyout.Server.Utils;
using MoreFlyout.Server.ViewModels;

namespace MoreFlyout.Server.Views;

public sealed partial class MediaFlyout : UserControl
{
    public MediaFlyoutViewModel ViewModel { get; set; }

    // DispatcherTimer to instead Timer
    private readonly DispatcherTimer _hiddenTimer;

    public MediaFlyout()
    {
        ViewModel = new MediaFlyoutViewModel();
        InitializeComponent();

        Unloaded += (s, e) =>
        {
            ViewModel.Dispose();
        };

        // Set DispatcherTimer to control the window will disappear after a certain period of time
        var timeoutSeconds = ConfigManager.Instance.MediaFlyoutSettings.TimeoutHiding;
        _hiddenTimer = new DispatcherTimer { Interval = new TimeSpan(0, 0, 0, 0, timeoutSeconds) };
        _hiddenTimer.Stop();
    }

    public void InitializeFlyout(int vkCode)
    {
        var flyoutEnabled = !ConfigManager.Instance.ServiceSettings.GameMode || !Screen.IsFullScreenActive();
        if (!App.FlyoutControl.IsOpen && flyoutEnabled)
        {
            App.FlyoutControl.Show();
        }

        RunTimer();
    }

    private void RunTimer()
    {
        if (!_hiddenTimer.IsEnabled)
        {
            _hiddenTimer.Start();
            _hiddenTimer.Tick += (sender, e) =>
            {
                if (App.FlyoutControl.IsOpen)
                {
                    App.FlyoutControl.Hide();
                }
                _hiddenTimer.Stop();
            };
        }
        else
        {
            _hiddenTimer.Stop();
        }
        _hiddenTimer.Start();
    }

    private void MediaGrid_PointerEntered(object sender, Microsoft.UI.Xaml.Input.PointerRoutedEventArgs e)
    {
        _hiddenTimer.Stop();
    }

    private void MediaGrid_PointerExited(object sender, Microsoft.UI.Xaml.Input.PointerRoutedEventArgs e)
    {
        _hiddenTimer.Start();
    }
}
