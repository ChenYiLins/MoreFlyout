using System.Diagnostics;
using MoreFlyout.Config;
using MoreFlyout.Server.Utils;
using MoreFlyout.Server.Views;

namespace MoreFlyout.Server;

public partial class App : Application
{
    public static FlyoutControl? FlyoutControl { get; private set; }
    public static TrayIcon? TrayIcon { get; private set; }
    public static FlyoutMoudles? FlyoutMoudles { get; private set; }

    private static Logger? _Logger;
    private static readonly Mutex _Mutex = new(false, "24043650-DED6-4E6B-8AFF-6BB03DFE3BDA");

    public App()
    {
        CheckServiceMutex();

        InitializeComponent();

        //Set up Logger
        var logDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "MoreFlyout");
        NLog.GlobalDiagnosticsContext.Set("logDir", logDir);

        _Logger = LogManager.GetCurrentClassLogger();
        _Logger.Info("NLog initialized successfully, and automatic achieving has been started (reserved for 7 days)");

        AppDomain.CurrentDomain.ProcessExit += OnProcessExit;

        UnhandledException += App_UnhandledException;
    }

    private static void CheckServiceMutex()
    {
        if (!_Mutex.WaitOne(TimeSpan.FromMilliseconds(50), false))
        {
            Debug.WriteLine("Another instance of the service is already running. Exiting this instance");
            Environment.Exit(-1);
        }
    }

    private void OnProcessExit(object? sender, EventArgs e)
    {
        FlyoutControl = null;
        FlyoutMoudles = null;
        TrayIcon = null;

        _Logger?.Info("Application is exiting");
        LogManager.Shutdown();
    }

    private void App_UnhandledException(object sender, Microsoft.UI.Xaml.UnhandledExceptionEventArgs e)
    {
        Debug.WriteLine($"Unhandled exception: {e.Exception.Message}");
    }

    protected override void OnLaunched(LaunchActivatedEventArgs args)
    {
        Current.DispatcherShutdownMode = DispatcherShutdownMode.OnExplicitShutdown;

        if (ConfigManager.Instance.ServiceSettings.AutoStart && !AutoStart.CheckAutoStart())
        {
            AutoStart.SetAutoStart(true);
        }

        FlyoutControl = new FlyoutControl();

        FlyoutMoudles = new FlyoutMoudles();

        _Logger?.Info("Flyout window activated");

        if (ConfigManager.Instance.ServiceSettings.ShowTrayIcon)
        {
            InitializeTrayIcon();
        }
    }

    private static void InitializeTrayIcon()
    {
        try
        {
            TrayIcon = new TrayIcon(1, "Assets/AppIcon.ico", "MoreFlyout") { IsVisible = ConfigManager.Instance.ServiceSettings.ShowTrayIcon };

            var trayIconMenuFlyout = new TrayIconMenuFlyout();

            foreach (var item in (trayIconMenuFlyout.ContextFlyout as MenuFlyout)!.Items)
            {
                if (item is not MenuFlyoutSeparator)
                {
                    item.Height = 32;
                    item.Padding = new Thickness(11, 0, 11, 0);
                }
            }

            TrayIcon.ContextMenu += (window, eventArgs) =>
            {
                eventArgs.Flyout = trayIconMenuFlyout.ContextFlyout;
            };

            _Logger?.Info("TrayIcon initiliazed");
        }
        catch (Exception ex)
        {
            _Logger?.Error($"TrayIcon initialization exception: {ex.Message}");
        }
    }
}
