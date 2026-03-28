using System.Diagnostics;
using System.Runtime.InteropServices;
using Microsoft.UI.Windowing;
using MoreFlyout.App.Contracts.Services;
using Windows.UI;

namespace MoreFlyout.App;

public sealed partial class MainWindow : Window
{
    public MainWindow(INavigationService navigationService)
    {
        InitializeComponent();

        ExtendsContentIntoTitleBar = true;
        SetTitleBar(AppTitleBar);
        AppTitleBar.Title = "MoreFlyout";
        AppTitleBar.Subtitle = Debugger.IsAttached ? "Debug" : "";
        AppTitleBar.ActualThemeChanged += (s, e) => ApplySystemThemeToCaptionButtons();

        ApplySystemThemeToCaptionButtons();

        var hWnd = WinRT.Interop.WindowNative.GetWindowHandle(this);
        var dpiForWindow = GetDpiForWindow(hWnd);
        var scaleFactor = dpiForWindow / 96.0;
        if (AppWindow.Presenter is OverlappedPresenter presenter)
        {
            presenter.PreferredMinimumWidth = (int)(920 * scaleFactor);
            presenter.PreferredMinimumHeight = (int)(600 * scaleFactor);
            presenter.PreferredMaximumHeight = 10000;
            presenter.PreferredMaximumWidth = 10000;
        }

        navigationService.Frame = NavigationFrame;
        navigationService.InitializeNavigationView(NavigationViewControl);
        navigationService.InitializeBreadcrumbBar(BreadcrumbBarControl);
        return;

        [DllImport("user32.dll")]
        static extern uint GetDpiForWindow([In] IntPtr hwnd);
    }

    private void NavViewTitleBar_BackRequested(Microsoft.UI.Xaml.Controls.TitleBar sender, object args)
    {
        if (NavigationFrame.CanGoBack)
        {
            NavigationFrame.GoBack();
        }
    }

    private void NavViewTitleBar_PaneToggleRequested(Microsoft.UI.Xaml.Controls.TitleBar sender, object args)
    {
        NavigationViewControl.IsPaneOpen = !NavigationViewControl.IsPaneOpen;
    }

    private void ApplySystemThemeToCaptionButtons()
    {
        var backgroundHoverColor = AppTitleBar.ActualTheme == ElementTheme.Dark ? Color.FromArgb(20, 255, 255, 255) : Color.FromArgb(40, 0, 0, 0);
        AppWindow.TitleBar.ButtonHoverBackgroundColor = backgroundHoverColor;
    }
}
