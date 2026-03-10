using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Win32;

namespace MoreFlyout.App.Utils;

internal class VersionInfo
{
    private const string WindowsNtCurrentVersionPath = @"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows NT\CurrentVersion";

    public static string Service => ValueOrNotFound(() => FileVersionInfo.GetVersionInfo(GetExecutionPath("server", "MoreFlyout.Server.exe"))?.FileVersion);
    public static string App => ValueOrNotFound(() => FileVersionInfo.GetVersionInfo(GetExecutionPath("app", "MoreFlyout.App.exe"))?.FileVersion);

    [RequiresAssemblyFiles("Calls MoreFlyout.App.Utils.VersionInfo.CommitHash()")]
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
        var currentPath = AppContext.BaseDirectory;
        var directoryInfo = new DirectoryInfo(currentPath);

        if (directoryInfo.Name.Equals("server", StringComparison.OrdinalIgnoreCase) || directoryInfo.Name.Equals("app", StringComparison.OrdinalIgnoreCase))
        {
            directoryInfo = directoryInfo.Parent ?? throw new InvalidOperationException("Parent directory is missing.");
        }

        if (!directoryInfo.Name.Equals("app", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException($"Expected directory 'app' but found '{directoryInfo.Name}'");
        }

        return directoryInfo.FullName;
    }

    [RequiresAssemblyFiles("Calls System.Reflection.Assembly.Location")]
    private static string GetCommitHash()
    {
        try
        {
            System.Reflection.Assembly assembly = System.Reflection.Assembly.GetExecutingAssembly();
            string productVersion = FileVersionInfo.GetVersionInfo(assembly.Location).ProductVersion ?? "None";
            string commitHash = productVersion[(productVersion.LastIndexOf('-') + 2)..];
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
