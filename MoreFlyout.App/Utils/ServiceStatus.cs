using System.Diagnostics;
using MoreFlyout.Comms;

namespace MoreFlyout.App.Utils;

internal static class ServiceStatus
{
    public static async Task<bool> VerifyServiceStartupAsync(int maxRetries = 5)
    {
        if(Debugger.IsAttached)
        {
            return true;
        }

        for (var i = 0; i < maxRetries; i++)
        {
            var serviceResponse = await PipeClient.SendMessageAndGetReplyAsync(new Message() { Type = MessageType.QueryService });
            if (serviceResponse is { Type: MessageType.ServiceResponse, Content: ResponseType.Ok })
            {
                return true;
            }
            await Task.Delay(1000);
        }

        return false;
    }

    public static async Task<bool> VerifyServiceRunningAsync(int maxRetries = 3)
    {
        var successTimes = 0;
        for(var i = 0; i < maxRetries; i++)
        {
            var processes = Process.GetProcessesByName("MoreFlyout.Service");
            if(processes.Length > 0)
            {
                successTimes++;
            }
            await Task.Delay(500);
        }

        return successTimes == maxRetries;
    }

    public static bool VerifyServicePath()
    {
        var directoryInfo = new DirectoryInfo(Environment.CurrentDirectory).Parent;
        if (directoryInfo is not null)
        {
            var servicePath = Path.Combine(directoryInfo.FullName, "service", "MoreFlyout.Service.exe");
            return File.Exists(servicePath);
        }
        return false;
    }
}
