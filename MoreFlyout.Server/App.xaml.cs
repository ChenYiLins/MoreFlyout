using System.Diagnostics;
using MoreFlyout.Config;
using MoreFlyout.Server.Services;
using MoreFlyout.Server.Utils;
using MoreFlyout.Server.Views;

namespace MoreFlyout.Server;

public partial class App : Application
{
    public static FlyoutControl? FlyoutControl { get; private set; }
    public static TrayIcon? TrayIcon { get; private set; }
    public static FlyoutMoudles? FlyoutMoudles { get; private set; }

    private static Logger? Logger;
    private static readonly Mutex Mutex = new(false, "24043650-DED6-4E6B-8AFF-6BB03DFE3BDA");

    private PipeCommService? _commService;

    public static void ReleaseSingleInstanceMutex()
    {
        try
        {
            Mutex.ReleaseMutex();
        }
        catch
        {
            // May not be called from the owning thread; ignore.
        }

        Mutex.Dispose();
    }

    public App()
    {
        CheckServiceMutex();

        InitializeComponent();

        //Set up Logger
        var logDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "MoreFlyout");
        NLog.GlobalDiagnosticsContext.Set("logDir", logDir);

        Logger = LogManager.GetCurrentClassLogger();
        Logger.Info("NLog initialized successfully, and automatic achieving has been started (reserved for 7 days)");

        AppDomain.CurrentDomain.ProcessExit += OnProcessExit;

        UnhandledException += App_UnhandledException;
    }

    private static void CheckServiceMutex()
    {
        try
        {
            if (!Mutex.WaitOne(TimeSpan.FromMilliseconds(50), false))
            {
                Debug.WriteLine("Another instance of the service is already running. Exiting this instance");
                Mutex.Dispose();
                Application.Current.Exit();
            }
        }
        catch (AbandonedMutexException)
        {
            // Previous instance disposed the mutex during restart.
            // WaitOne still grants ownership, so we can safely continue.
        }
    }

    private void OnProcessExit(object? sender, EventArgs e)
    {
        FlyoutControl = null;
        FlyoutMoudles = null;
        TrayIcon = null;

        _commService?.Stop();

        Logger?.Info("Application is exiting");
        LogManager.Shutdown();
    }

    private void App_UnhandledException(object sender, Microsoft.UI.Xaml.UnhandledExceptionEventArgs e)
    {
        Debug.WriteLine($"Unhandled exception: {e.Exception.Message}");
    }

    protected override void OnLaunched(LaunchActivatedEventArgs args)
    {
        Current.DispatcherShutdownMode = DispatcherShutdownMode.OnExplicitShutdown;

        // Run as a tray service; lower priority so the OS deprioritizes us when idle
        Process.GetCurrentProcess().PriorityClass = ProcessPriorityClass.BelowNormal;

        Microsoft.Windows.Globalization.ApplicationLanguages.PrimaryLanguageOverride = ConfigManager.Instance.ServiceSettings.SelectedLanguageCode;

        if (ConfigManager.Instance.ServiceSettings.AutoStart && !AutoStart.CheckAutoStart())
        {
            AutoStart.SetAutoStart(true);
        }

        FlyoutControl = new FlyoutControl();

        FlyoutMoudles = new FlyoutMoudles();

        Logger?.Info("Flyout window activated");

        // Enable communication service
        _commService = new PipeCommService();
        _ = _commService.StartAsync();

        InitializeTrayIcon();
    }

    private static void InitializeTrayIcon()
    {
        try
        {
            TrayIcon = new TrayIcon(1, "Assets/AppIcon.ico", "MoreFlyout") { IsVisible = !ConfigManager.Instance.ServiceSettings.HideTrayIcon };

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

            Logger?.Info("TrayIcon initiliazed");
        }
        catch (Exception ex)
        {
            Logger?.Error($"TrayIcon initialization exception: {ex.Message}");
        }
    }

    public static void Dispose()
    {
        App.FlyoutMoudles?.Dispose();
        App.FlyoutControl?.Dispose();
        App.TrayIcon?.Dispose();
    }
}
