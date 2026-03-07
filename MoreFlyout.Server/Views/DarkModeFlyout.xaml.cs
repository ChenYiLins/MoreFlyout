using Microsoft.UI.Xaml.Media.Animation;
using MoreFlyout.Config;
using MoreFlyout.Server.Helpers;
using MoreFlyout.Server.Utils;
using MoreFlyout.Server.ViewModels;

namespace MoreFlyout.Server.Views;

public sealed partial class DarkModeFlyout : UserControl
{
    public DarkModeFlyoutViewModel ViewModel { get; }

    // Timer helper for auto-hide functionality
    private readonly FlyoutTimerHelper _timerHelper;

    public DarkModeFlyout()
    {
        ViewModel = new DarkModeFlyoutViewModel();
        InitializeComponent();

        // Set up timer to control the window will disappear after a certain period of time
        var timeoutSeconds = ConfigManager.Instance.DarkModeFlyoutSettings.TimeoutHiding;
        _timerHelper = new FlyoutTimerHelper(timeoutSeconds, () =>
        {
            if (App.FlyoutControl?.IsOpen == true && !ToggleModeButton.Flyout.IsOpen)
            {
                App.FlyoutControl.Hide();
            }
        });
    }

    public void InitializeFlyout(bool darkModeEnabled)
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

        StatusTextBlock.Text = darkModeEnabled ? "DarkMode".GetLocalized() : "LightMode".GetLocalized();
        _ = ChangeGlyphWithAnimation(darkModeEnabled ? "\uE708" : "\uE706");

        _timerHelper.Start();
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
        _timerHelper.Stop();
    }

    private void DarkModeGrid_PointerExited(object sender, Microsoft.UI.Xaml.Input.PointerRoutedEventArgs e)
    {
        _timerHelper.Start();
    }
}
