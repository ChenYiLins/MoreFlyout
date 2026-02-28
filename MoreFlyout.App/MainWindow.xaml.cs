using System.Diagnostics;
using System.Runtime.InteropServices;
using Microsoft.UI.Windowing;
using MoreFlyout.App.Contracts.Services;
using Windows.UI;

namespace MoreFlyout.App;

public sealed partial class MainWindow : Window
{
    private readonly INavigationService _navigationService;

    public MainWindow(INavigationService navigationService)
    {
        _navigationService = navigationService;
        InitializeComponent();

        ExtendsContentIntoTitleBar = true;
        SetTitleBar(AppTitleBar);
        AppTitleBar.Title = "MoreFlyout";
        AppTitleBar.Subtitle = Debugger.IsAttached ? "Debug" : "";
        AppTitleBar.ActualThemeChanged += (s, e) => ApplySystemThemeToCaptionButtons();

        ApplySystemThemeToCaptionButtons();

        [DllImport("user32.dll")]
        static extern uint GetDpiForWindow([In] IntPtr hwnd);
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

        _navigationService.Frame = NavigationFrame;
        _navigationService.InitializeNavigationView(NavigationViewControl);
        _navigationService.InitializeBreadcrumbBar(BreadcrumbBarControl);
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
