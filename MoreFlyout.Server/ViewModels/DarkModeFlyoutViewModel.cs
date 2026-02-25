using CommunityToolkit.Mvvm.Input;

namespace MoreFlyout.Server.ViewModels;

public partial class DarkModeFlyoutViewModel : ObservableObject
{
    [RelayCommand]
    private void PauseAutoSwitch()
    {
        CallShellCommand("--toggle-skip-next");
    }

    [RelayCommand]
    private void ToggleTheme()
    {
        CallShellCommand("--swap");
    }

    [RelayCommand]
    private void ForceLightTheme()
    {
        CallShellCommand("--force-light");
    }

    [RelayCommand]
    private void ForceDarkTheme()
    {
        CallShellCommand("--force-dark");
    }

    public DarkModeFlyoutViewModel() { }

    private static void CallShellCommand(string args)
    {
        var logDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), @"Programs\AutoDarkMode\adm-app\core\AutoDarkModeShell.exe");
        if (File.Exists(logDir))
        {
            System.Diagnostics.Process.Start(
                new System.Diagnostics.ProcessStartInfo
                {
                    FileName = logDir,
                    UseShellExecute = false,
                    Arguments = args,
                    CreateNoWindow = true,
                }
            );
        }
    }
}
