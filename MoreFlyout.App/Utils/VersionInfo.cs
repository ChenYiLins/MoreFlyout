using System.Diagnostics;
using Microsoft.Win32;

namespace MoreFlyout.App.Utils;

internal class VersionInfo
{
    private const string WindowsNtCurrentVersionPath = @"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows NT\CurrentVersion";
    private const string ServerFolderName = "server";
    private const string ServerExeName = "MoreFlyout.Server.exe";
    private const string AppFolderName = "app";
    private const string AppExeName = "MoreFlyout.App.exe";
    private const string NotFound = "not found";
    private const string UbrRegistryName = "UBR";

    public static string Service => GetFileVersion(ServerFolderName, ServerExeName);
    public static string App => GetFileVersion(AppFolderName, AppExeName);
    public static string NetCore => ValueOrDefault(() => Environment.Version.ToString());
    public static string WindowsVersion => ValueOrDefault(() => $"{Environment.OSVersion.Version.Build}.{GetRegistryValue(WindowsNtCurrentVersionPath, UbrRegistryName, "0") ?? "0"}");
    public static string Architecture => ValueOrDefault(() => Environment.Is64BitOperatingSystem ? "x64" : "x86");

    private static string GetFileVersion(string folderPath, string fileName)
    {
        return ValueOrDefault(() => FileVersionInfo.GetVersionInfo(GetExecutionPath(folderPath, fileName))?.FileVersion);
    }

    private static string ValueOrDefault(Func<string?> getValue)
    {
        try
        {
            return getValue() ?? NotFound;
        }
        catch
        {
            return NotFound;
        }
    }

    private static string GetExecutionPath(string folderPath, string fileName)
    {
        string basePath = GetBasePath();
        return Path.Combine(basePath, folderPath, fileName);
    }

    private static string GetBasePath()
    {
        string currentPath = Environment.CurrentDirectory;
        string parentPath = new DirectoryInfo(currentPath).Parent?.FullName ?? currentPath;
        return parentPath;
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
