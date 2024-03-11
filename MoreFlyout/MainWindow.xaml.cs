using Microsoft.UI.Composition.SystemBackdrops;
using Microsoft.UI.Windowing;
using MoreFlyout.Helpers;
using Windows.UI.ViewManagement;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.UI.WindowsAndMessaging;
using WinRT;

namespace MoreFlyout;

public sealed partial class MainWindow : WindowEx
{
    private readonly Microsoft.UI.Dispatching.DispatcherQueue dispatcherQueue;

    private readonly SystemBackdropConfiguration m_configurationSource;
    private readonly DesktopAcrylicController m_backdropController;

    private readonly UISettings settings;

    public MainWindow()
    {
        InitializeComponent();
        Content = null;

        //  apply Acrylic for MainWindow
        m_configurationSource = new SystemBackdropConfiguration()
        {
            IsInputActive = true,
            Theme = SystemBackdropTheme.Dark
        };
        m_backdropController = new DesktopAcrylicController();
        m_backdropController.AddSystemBackdropTarget(this.As<Microsoft.UI.Composition.ICompositionSupportsSystemBackdrop>());
        m_backdropController.SetSystemBackdropConfiguration(m_configurationSource);

        //  change window style
        var presenter = AppWindow.Presenter as OverlappedPresenter;
        presenter!.IsResizable = false;
        presenter!.SetBorderAndTitleBar(false, false);

        // set window position
        var hWnd = (HWND)WinRT.Interop.WindowNative.GetWindowHandle(this);
        var screenWidth = PInvoke.GetSystemMetrics(SYSTEM_METRICS_INDEX.SM_CXSCREEN);
        var screenHeight = PInvoke.GetSystemMetrics(SYSTEM_METRICS_INDEX.SM_CYSCREEN);
        var dpiForWindow = PInvoke.GetDpiForWindow(hWnd);//获取系统DPI
        var windowRatio = dpiForWindow / 96.0;
        AppWindow.Resize(new Windows.Graphics.SizeInt32((int)(195 * windowRatio), (int)(48 * windowRatio)));
        AppWindow.Move(new Windows.Graphics.PointInt32(screenWidth / 2 - (int)(195 / 2 * windowRatio), screenHeight - (int)(108 * windowRatio)));

        // Theme change code picked from https://github.com/microsoft/WinUI-Gallery/pull/1239
        dispatcherQueue = Microsoft.UI.Dispatching.DispatcherQueue.GetForCurrentThread();
        settings = new UISettings();
        settings.ColorValuesChanged += Settings_ColorValuesChanged; // cannot use FrameworkElement.ActualThemeChanged event
    }

    // this handles updating the caption button colors correctly when windows system theme is changed
    // while the app is open
    private void Settings_ColorValuesChanged(UISettings sender, object args)
    {
        // This calls comes off-thread, hence we will need to dispatch it to current app's thread
        dispatcherQueue.TryEnqueue(() =>
        {
            TitleBarHelper.ApplySystemThemeToCaptionButtons();
        });
    }

}
