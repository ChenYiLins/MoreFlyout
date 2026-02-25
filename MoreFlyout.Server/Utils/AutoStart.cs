using Microsoft.Win32;

namespace MoreFlyout.Server.Utils
{
    internal class AutoStart
    {
        private static readonly Logger _Logger = LogManager.GetCurrentClassLogger();

        private const string RunKey = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run";

        /// <summary>
        /// Enables or disables automatic startup of the application when the user logs in to Windows.
        /// </summary>
        /// <remarks>This method modifies the current user's registry settings to add or remove the
        /// application from the list of programs that start automatically with Windows. Administrative privileges are
        /// not required, but the user must have permission to modify their own registry settings.</remarks>
        /// <param name="enabled">true to enable auto-start; false to disable it.</param>
        /// <returns>true if the auto-start setting was successfully updated; otherwise, false.</returns>
        public static bool SetAutoStart(bool enabled)
        {
            var exePath = Environment.ProcessPath;
            if (exePath is null)
            {
                _Logger.Error("Failed to get the executable path for auto-start configuration");
                return false;
            }

            try
            {
                using var key = Registry.CurrentUser.OpenSubKey(RunKey, true);
                if (key is null)
                {
                    _Logger.Error("Failed to open registry key for auto-start configuration");
                    return false;
                }

                if (enabled)
                {
                    key.SetValue("MoreFlyout", exePath);
                    _Logger.Info($"Enabled auto-start for MoreFlyout with executable path: {exePath}");
                }
                else
                {
                    if (key.GetValue("MoreFlyout") is not null)
                    {
                        key.DeleteValue("MoreFlyout");
                        _Logger.Info("Disabled auto-start for MoreFlyout");
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                _Logger.Error(ex, $"Failed to update auto-start configuration in the registry: {ex.Message}");
                return false;
            }
        }

        public static bool CheckAutoStart()
        {
            try
            {
                using var key = Registry.CurrentUser.OpenSubKey(RunKey, true);
                if (key is null)
                {
                    _Logger.Error("Failed to open registry key for auto-start configuration");
                    return false;
                }

                return key.GetValue("MoreFlyout") is not null;
            }
            catch (Exception ex)
            {
                _Logger.Error(ex, $"Failed to check auto-start configuration in the registry: {ex.Message}");
                return false;
            }
        }
    }
}
