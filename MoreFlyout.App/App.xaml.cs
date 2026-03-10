using Microsoft.Extensions.DependencyInjection;
using MoreFlyout.App.Contracts.Services;
using MoreFlyout.App.Services;

namespace MoreFlyout.App;

public partial class App : Application
{
    public static Window MainWindow { get; set; } = Window.Current;
    public IServiceProvider Services { get; }

    public static T GetService<T>()
        where T : class
    {
        return (App.Current as App)!.Services.GetService(typeof(T)) is not T service
            ? throw new ArgumentException($"{typeof(T)} needs to be registered in ConfigureServices within App.xaml.cs.")
            : service;
    }

    public App()
    {
        InitializeComponent();

        Services = ConfigureServices();
    }

    private static ServiceProvider ConfigureServices()
    {
        var services = new ServiceCollection();

        services.AddSingleton<MainWindow>();
        services.AddSingleton<IActivationService, ActivationService>();
        services.AddSingleton<ICloseService, CloseService>();
        services.AddSingleton<IPageService, PageService>();
        services.AddSingleton<INavigationService, NavigationService>();

        services.AddTransient<HomeViewModel>();
        services.AddTransient<AboutViewModel>();
        services.AddTransient<SettingsViewModel>();

        services.AddTransient<KeyIndicatorViewModel>();
        services.AddTransient<MediaViewModel>();
        services.AddTransient<DarkModeViewModel>();

        return services.BuildServiceProvider();
    }

    protected override void OnLaunched(LaunchActivatedEventArgs args)
    {
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
