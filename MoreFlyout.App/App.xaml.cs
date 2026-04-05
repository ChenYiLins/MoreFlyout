using System.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using MoreFlyout.App.Contracts.Services;
using MoreFlyout.App.Helpers;
using MoreFlyout.App.Services;

namespace MoreFlyout.App;

public partial class App : Application
{
    public static Window MainWindow { get; set; } = Window.Current;
    public IServiceProvider Services { get; }

    private static readonly Mutex Mutex = new(false, "{A41B77F1-2820-4907-AF43-A9FD254D2ED2}");

    public static T GetService<T>()
        where T : class
    {
        return (App.Current as App)!.Services.GetService(typeof(T)) is not T service
            ? throw new ArgumentException($"{typeof(T)} needs to be registered in ConfigureServices within App.xaml.cs.")
            : service;
    }

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
        CheckAppMutex();

        InitializeComponent();

        Services = ConfigureServices();
    }

    public static void CheckAppMutex()
    {
        try
        {
            if (!Mutex.WaitOne(TimeSpan.FromMilliseconds(50), false) && !Debugger.IsAttached)
            {
                List<Process> processes = [.. Process.GetProcessesByName("MoreFlyout.App")];
                if (processes.Count > 0)
                {
                    Helpers.WindowHelper.BringProcessToFront(processes[0]);
                    Mutex.Dispose();
                    Application.Current.Exit();
                }
            }
        }
        catch (AbandonedMutexException)
        {
            // Previous instance disposed the mutex during restart.
            // WaitOne still grants ownership, so we can safely continue.
        }
    }

    private static ServiceProvider ConfigureServices()
    {
        var services = new ServiceCollection();

        services.AddSingleton<MainWindow>();
        services.AddSingleton<IActivationService, ActivationService>();
        services.AddSingleton<ICloseService, CloseService>();
        services.AddSingleton<IPageService, PageService>();
        services.AddSingleton<INavigationService, NavigationService>();
        services.AddSingleton<IErrorService, ErrorService>();

        services.AddTransient<HomeViewModel>();
        services.AddTransient<ConditionViewModel>();
        services.AddTransient<AboutViewModel>();
        services.AddTransient<SettingsViewModel>();

        services.AddTransient<KeyIndicatorViewModel>();
        services.AddTransient<MediaViewModel>();
        services.AddTransient<DarkModeViewModel>();

        return services.BuildServiceProvider();
    }

    protected override void OnLaunched(LaunchActivatedEventArgs args)
    {
        Microsoft.Windows.Globalization.ApplicationLanguages.PrimaryLanguageOverride = LanguageHelper.GetDefaultLanguage();

        MainWindow = App.GetService<MainWindow>();

        MainWindow.Closed += MainWindow_Closed; ;

        _ = ActivateAsync(args);
    }

    private void MainWindow_Closed(object sender, WindowEventArgs args)
    {
        App.GetService<ICloseService>().Close();
    }

    private static async Task ActivateAsync(LaunchActivatedEventArgs args)
    {
        await App.GetService<IActivationService>().ActivateAsync(args);
    }
}
