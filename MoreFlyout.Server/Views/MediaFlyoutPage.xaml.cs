using MoreFlyout.Config;
using MoreFlyout.Server.Utils;
using MoreFlyout.Server.ViewModels;

namespace MoreFlyout.Server.Views;

public sealed partial class MediaFlyoutPage : Page
{
    public MediaFlyoutViewModel ViewModel { get; set; }

    // DispatcherTimer to instead Timer
    private readonly DispatcherTimer _hiddenTimer;

    public MediaFlyoutPage()
    {
        ViewModel = new MediaFlyoutViewModel();
        InitializeComponent();
        RequestedTheme = SystemTheme.GetCurrentSystemTheme();

        Unloaded += (s, e) =>
        {
            ViewModel.Dispose();
        };

        // Set DispatcherTimer to control the window will disappear after a certain period of time
        var timeoutSeconds = ConfigManager.Instance.MediaFlyout.TimeoutHiding;
        _hiddenTimer = new DispatcherTimer { Interval = new TimeSpan(0, 0, 0, 0, timeoutSeconds) };
        _hiddenTimer.Stop();
    }

    public void InitializeFlyout(int vkCode)
    {
        bool flyoutEnabled = !ConfigManager.Instance.ServiceSettings.GameMode || !Screen.IsFullScreenActive();
        if (!PageContextFlyout.IsOpen && flyoutEnabled)
        {
            PageContextFlyout.ShowAt(this);
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
                if (PageContextFlyout.IsOpen)
                {
                    PageContextFlyout.Hide();
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
