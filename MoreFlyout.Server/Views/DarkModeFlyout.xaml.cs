using Microsoft.UI.Xaml.Media.Animation;
using MoreFlyout.Config;
using MoreFlyout.Server.Helpers;
using MoreFlyout.Server.Utils;
using MoreFlyout.Server.ViewModels;

namespace MoreFlyout.Server.Views;

public sealed partial class DarkModeFlyout : UserControl
{
    public DarkModeFlyoutViewModel ViewModel { get; }

    // DispatcherTimer to instead Timer
    private readonly DispatcherTimer _hiddenTimer;

    public DarkModeFlyout()
    {
        ViewModel = new DarkModeFlyoutViewModel();
        InitializeComponent();
        RequestedTheme = SystemTheme.GetCurrentSystemTheme();

        // Set DispatcherTimer to control the window will disappear after a certain period of time
        var timeoutSeconds = ConfigManager.Instance.DarkModeFlyoutSettings.TimeoutHiding;
        _hiddenTimer = new DispatcherTimer { Interval = new TimeSpan(0, 0, 0, 0, timeoutSeconds) };
        _hiddenTimer.Stop();
    }

    public void InitializeFlyout(bool darkModeEnabled)
    {
        bool flyoutEnabled = !ConfigManager.Instance.ServiceSettings.GameMode || !Screen.IsFullScreenActive();
        if (!App.FlyoutControl.IsOpen && flyoutEnabled)
        {
            App.FlyoutControl.Show();
        }

        StatusTextBlock.Text = darkModeEnabled ? "DarkMode".GetLocalized() : "LightMode".GetLocalized();
        _ = ChangeGlyphWithAnimation(darkModeEnabled ? "\uE708" : "\uE706");

        RunTimer();
    }

    private void RunTimer()
    {
        if (!_hiddenTimer.IsEnabled)
        {
            _hiddenTimer.Start();
            _hiddenTimer.Tick += (sender, e) =>
            {
                if (App.FlyoutControl.IsOpen && !ToggleModeButton.Flyout.IsOpen)
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

    private async Task ChangeGlyphWithAnimation(string newGlyph)
    {
        await Task.Delay(800);

        var fadeOut = new DoubleAnimation
        {
            From = 1,
            To = 0,
            Duration = new Duration(TimeSpan.FromMilliseconds(150)),
        };
        var sb1 = new Storyboard();
        sb1.Children.Add(fadeOut);
        Storyboard.SetTarget(fadeOut, LightDarkModeFontIcon);
        Storyboard.SetTargetProperty(fadeOut, "Opacity");
        sb1.Begin();

        await Task.Delay(150);

        LightDarkModeFontIcon.Glyph = newGlyph;

        var fadeIn = new DoubleAnimation
        {
            From = 0,
            To = 1,
            Duration = new Duration(TimeSpan.FromMilliseconds(150)),
        };
        var sb2 = new Storyboard();
        sb2.Children.Add(fadeIn);
        Storyboard.SetTarget(fadeIn, LightDarkModeFontIcon);
        Storyboard.SetTargetProperty(fadeIn, "Opacity");
        sb2.Begin();
    }

    private void DarkModeGrid_PointerEntered(object sender, Microsoft.UI.Xaml.Input.PointerRoutedEventArgs e)
    {
        _hiddenTimer.Stop();
    }

    private void DarkModeGrid_PointerExited(object sender, Microsoft.UI.Xaml.Input.PointerRoutedEventArgs e)
    {
        _hiddenTimer.Start();
    }
}
