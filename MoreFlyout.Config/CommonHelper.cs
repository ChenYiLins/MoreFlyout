namespace MoreFlyout.Config;

public static class CommonHelper
{
    public static readonly string ExecutionPathApp = Path.Combine(Directory.GetParent(Environment.ProcessPath!)!.Parent!.FullName, "app", "MoreFlyout.App.exe");

    public static readonly string ExecutionPathServer = Path.Combine(Directory.GetParent(Environment.ProcessPath!)!.Parent!.FullName, "server", "MoreFlyout.Server.exe");

    public static readonly string ConfigPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "MoreFlyout", "config.yaml");

    public static readonly string ConfigFolderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "MoreFlyout");

}
