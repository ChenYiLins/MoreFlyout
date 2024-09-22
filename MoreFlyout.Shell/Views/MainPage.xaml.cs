using System.Diagnostics;
using Microsoft.UI.Xaml.Controls;
using Microsoft.Win32;
using MoreFlyout.Shell.Helpers;
using MoreFlyout.Shell.ViewModels;

namespace MoreFlyout.Shell.Views;

public sealed partial class MainPage : Page
{
    public MainViewModel ViewModel
    {
        get;
    }

    public MainPage()
    {
        ViewModel = App.GetService<MainViewModel>();
        InitializeComponent();

        AppTitleBarText.Text = "AppDisplayName".GetLocalized();

        // Check MoreFlyout.Server is running?
        var isProcessRunning = false;
        foreach (var process in Process.GetProcesses())
        {
            if (process.ProcessName.Equals("MoreFlyout.Server", StringComparison.CurrentCultureIgnoreCase))
            {
                isProcessRunning = true;
                break;
            }
        }

        if (isProcessRunning == true)
        {
            RunServerToggleSwitch.IsOn = true;
        }

        // Check MoreFlyout.Server is run with windows?
        using var key = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
        if (key != null)
        {
            RunWithWindowsToggleSwitch.IsOn = true;
        }

    }

    private void ToggleSwitch_Server_Toggled(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        var toggleSwitch = sender as ToggleSwitch;
        if (toggleSwitch != null)
        {
            if (toggleSwitch.IsOn == true)
            {
                try
                {
                    Process.Start(AppDomain.CurrentDomain.BaseDirectory.Substring(0, AppDomain.CurrentDomain.BaseDirectory.Length - @"MoreFlyout.Shell\".Length) + @"MoreFlyout.Server\MoreFlyout.Server.exe");
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error terminating processes: {ex.Message}");
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
    }

    private void ToggleSwitch_RunWithWindows_Toggled(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        var toggleSwitch = sender as ToggleSwitch;
        if (toggleSwitch != null)
        {
            if (toggleSwitch.IsOn == true)
            {
                try
                {
                    // Open or create Registry key of "HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Run"
                    var key = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);

                    // MoreFlyout.Server.exe's path
                    var exePath = "\"" + AppDomain.CurrentDomain.BaseDirectory.Substring(0, AppDomain.CurrentDomain.BaseDirectory.Length - @"MoreFlyout.Shell\".Length) + @"MoreFlyout.Server\MoreFlyout.Server.exe" + "\"";

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
}
