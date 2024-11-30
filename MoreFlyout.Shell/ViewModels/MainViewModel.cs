using System.Diagnostics;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Win32;

namespace MoreFlyout.Shell.ViewModels;

public partial class MainViewModel : ObservableRecipient
{
    [ObservableProperty]
    private bool _isServerRunning;

    [ObservableProperty]
    private bool _isRunWithWindows;

    public MainViewModel()
    {
        _isServerRunning = Process.GetProcessesByName("MoreFlyout.Server").Length > 0;

        using var key = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
        _isRunWithWindows = key?.GetValue("MoreFlyout.Server") != null;
    }

    partial void OnIsServerRunningChanged(bool value)
    {
        if (value)
        {
            try
            {
                string baseDirectory = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory)
                   .Parent?.FullName ?? AppDomain.CurrentDomain.BaseDirectory;
                string serverPath = Path.Combine(baseDirectory, @"MoreFlyout.Server\MoreFlyout.Server.exe");
                Process.Start(serverPath);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error starting process: {ex.Message}");
            }
        }
        else
        {
            try
            {
                // Get all of processes named MoreFlyout.Server.exe
                var processes = Process.GetProcessesByName("MoreFlyout.Server");

                // try to kill every processes
                foreach (var process in processes)
                {
                    Debug.WriteLine($"Terminating process with ID: {process.Id}");
                    process.Kill();
                }

                Debug.WriteLine("All instances of MoreFlyout.Server have been terminated.");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error terminating processes: {ex.Message}");
            }
        }
    }

    partial void OnIsRunWithWindowsChanged(bool value)
    {
        if (value)
        {
            try
            {
                // Open or create Registry key of "HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Run"
                var key = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);

                // MoreFlyout.Server.exe's path
                var exePath = string.Concat("\"", AppDomain.CurrentDomain.BaseDirectory.AsSpan(0, AppDomain.CurrentDomain.BaseDirectory.Length - @"MoreFlyout.Shell\".Length), @"MoreFlyout.Server\MoreFlyout.Server.exe", "\"");

                // SetValue
                key?.SetValue("MoreFlyout.Server", exePath);

                Debug.WriteLine("MoreFlyout.Server has been added to the startup list successfully.");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"An error occurred while adding MoreFlyout.Server to the startup list: {ex.Message}");
            }
        }
        else
        {
            try
            {
                // Open Registry key of "HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Run"
                using var key = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
                if (key != null)
                {
                    // Delete "MoreFlyout.Server"
                    key.DeleteValue("MoreFlyout.Server", false);

                    Debug.WriteLine("The startup entry for MoreFlyout.Server has been removed successfully.");
                }
                else
                {
                    throw new InvalidOperationException("Could not open the registry key for startup entries.");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"An error occurred while removing the startup entry: {ex.Message}");
            }
        }
    }
}
