using Microsoft.Win32;

namespace MoreFlyout.Service.Utils;

internal class EnergySaver
{
    public static bool IsEnergySaverEnabled()
    {
        var key = Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\Control\Power");
        var enabled = key?.GetValue("EnergySaverState")?.Equals(1);
        if (enabled == true)
            return true;
        return false;
    }
}
