using System.IO.Pipes;
using System.Text;

namespace MoreFlyout.Comms;

/// <summary>
/// Pipe client class for sending messages from the App to the Server using named pipes for IPC communication.
/// </summary>
public class PipeClient
{
    private const string PipeName = "MoreFlyout.IPC";
    private const string PipeServerName = ".";
    private const int ConnectTimeoutMs = 5000;

/// <summary>
/// Asynchronously sends a message to the named pipe server.
/// </summary>
/// <remarks>Returns <see langword="false"/> if the connection to the named pipe server fails or if an error
/// occurs during transmission.</remarks>
/// <param name="message">The message to send to the server. Cannot be null.</param>
/// <returns>A task that represents the asynchronous operation. The task result is <see langword="true"/> if the message was sent
/// successfully; otherwise, <see langword="false"/>.</returns>
    public static async Task<bool> SendMessageAsync(Message message)
    {
        try
        {
            using (var pipeClient = new NamedPipeClientStream(PipeServerName, PipeName, PipeDirection.Out))
            {
                await pipeClient.ConnectAsync(ConnectTimeoutMs);

                string json = message.Serialize();
                byte[] buffer = Encoding.UTF8.GetBytes(json);

                await pipeClient.WriteAsync(buffer, 0, buffer.Length);
                await pipeClient.FlushAsync();

                return true;
            }
        }
        catch
        {
            return false;
        }
    }
}
