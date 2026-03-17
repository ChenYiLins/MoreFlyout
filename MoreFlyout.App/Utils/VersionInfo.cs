using System.Diagnostics;
using Microsoft.Win32;

namespace MoreFlyout.App.Utils;

internal class VersionInfo
{
    private const string WindowsNtCurrentVersionPath = @"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows NT\CurrentVersion";

    public static string Service => ValueOrNotFound(() => FileVersionInfo.GetVersionInfo(GetExecutionPath("server", "MoreFlyout.Server.exe"))?.FileVersion);
    public static string App => ValueOrNotFound(() => FileVersionInfo.GetVersionInfo(GetExecutionPath("app", "MoreFlyout.App.exe"))?.FileVersion);
    public static string CommitHash => GetCommitHash();
    public static string NetCore => ValueOrNotFound(() => Environment.Version.ToString());
    public static string WindowsVersion => ValueOrNotFound(() => $"{Environment.OSVersion.Version.Build}.{GetRegistryValue(WindowsNtCurrentVersionPath, "UBR", "0") ?? "0"}");
    public static string Architecture => ValueOrNotFound(() => Environment.Is64BitOperatingSystem ? "x64" : "x86");

    public VersionInfo() { }

    private static string ValueOrNotFound(Func<string?> value)
    {
        try
        {
            return value() ?? "not found";
        }
        catch
        {
            return "not found";
        }
    }

    private static string GetExecutionPath(string folderPath, string programerPath)
    {
        var assemblyLocation = GetValidatedBasePath();
        return Path.Combine(assemblyLocation, folderPath, programerPath);
    }

    private static string GetValidatedBasePath()
    {
        var currentPath = Environment.CurrentDirectory;
        var directoryInfo = new DirectoryInfo(currentPath);

        return directoryInfo.Parent.FullName;
    }

    private static string GetCommitHash()
    {
        try
        {
            string appExePath = GetExecutionPath("app", "MoreFlyout.App.exe");
            string productVersion = FileVersionInfo.GetVersionInfo(appExePath).ProductVersion ?? "None";

            if (productVersion == "None")
            {
                return "None";
            }

            int lastDashIndex = productVersion.LastIndexOf('-');
            if (lastDashIndex < 0 || lastDashIndex + 2 >= productVersion.Length)
            {
                return "None";
            }

            string commitHash = productVersion[(lastDashIndex + 2)..];
            return commitHash;
        }
        catch
        {
            return "None";
        }
    }

    private static string GetRegistryValue(string path, string name, string defaultValue)
    {
        try
        {
            return Registry.GetValue(path, name, defaultValue)?.ToString() ?? defaultValue;
        }
        catch
        {
            return defaultValue;
        }
    }
}
