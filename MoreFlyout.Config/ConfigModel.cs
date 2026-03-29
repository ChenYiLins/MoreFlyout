namespace MoreFlyout.Config;

public class ConfigModel
{
    public AppSettings AppSettings { get; set; } = new();
    public ServiceSettings ServiceSettings { get; set; } = new();
    public KeyIndicatorFlyoutSettings KeyIndicatorFlyoutSettings { get; set; } = new();
    public MediaFlyoutSettings MediaFlyoutSettings { get; set; } = new();
    public DarkModeFlyoutSettings DarkModeFlyoutSettings { get; set; } = new();
}

public class AppSettings
{
    public bool FirstLaunch { get; set; } = true;
    public int[] WindowPositionAndSize { get; set; } = [0, 0, 1100, 650];

    /// <summary>
    /// Current state of the window.
    /// </summary>
    /// <remarks>The value typical representatives different window states, the maximum is 0, the minimum is 1, and the normal state is 2.</remarks>
    public int WindowState { get; set; } = 2;

    public string SelectedLanguageCode { get; set; } = "en-us";

    public bool LanguageChanged { get; set; } = false;
}

public class ServiceSettings
{
    public bool IsEnabled { get; set; } = true;

    /// <summary>
    /// Whether to start MoreFlyout automatically on system startup
    /// </summary>
    public bool AutoStart { get; set; } = false;

    public string? AutoStartPath { get; set; }

    /// <summary>
    /// Whether the application displays an icon in the system tray
    /// </summary>
    public bool HideTrayIcon { get; set; } = false;

    public bool GameMode { get; set; } = true;

    public string SelectedLanguageCode { get; set; } = "en-us";
}

public class KeyIndicatorFlyoutSettings
{
    public bool IsEnabled { get; set; } = true;
    public int TimeoutHiding { get; set; } = 2800;
}

public class MediaFlyoutSettings
{
    public bool IsEnabled { get; set; } = true;
    public int TimeoutHiding { get; set; } = 2800;
}

public class DarkModeFlyoutSettings
{
    public bool IsEnabled { get; set; } = false;
    public int TimeoutHiding { get; set; } = 2800;
}
