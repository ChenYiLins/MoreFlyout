namespace MoreFlyout.Config;

public class ConfigModel
{
    public AppSettings AppSettings { get; set; } = new();
    public ServiceSettings ServiceSettings { get; set; } = new();
    public KeyIndicatorFlyout KeyIndicatorFlyout { get; set; } = new();
    public MediaFlyout MediaFlyout { get; set; } = new();
    public DarkModeFlyout DarkModeFlyout { get; set; } = new();
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
}

public class ServiceSettings
{
    /// <summary>
    /// Whether to start MoreFlyout automatically on system startup
    /// </summary>
    public bool AutoStart { get; set; } = false;

    /// <summary>
    /// Whether the application displays an icon in the system tray
    /// </summary>
    public bool ShowTrayIcon { get; set; } = true;

    public bool GameMode { get; set; } = true;
}

public class KeyIndicatorFlyout
{
    public bool IsEnabled { get; set; } = true;
    public int TimeoutHiding { get; set; } = 2800;
}

public class MediaFlyout
{
    public bool IsEnabled { get; set; } = true;
    public int TimeoutHiding { get; set; } = 2800;
}

public class DarkModeFlyout
{
    public bool IsEnabled { get; set; } = false;
    public int TimeoutHiding { get; set; } = 2800;
}
