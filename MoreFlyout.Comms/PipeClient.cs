using System.IO.Pipes;
using System.Runtime.Versioning;
using System.Text;

namespace MoreFlyout.Comms;

/// <summary>
/// Pipe client class for sending messages from the App to the Server using named pipes for IPC communication.
/// </summary>
[SupportedOSPlatform("windows")]
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

    /// <summary>
    /// Asynchronously sends a message to the named pipe server and waits for a reply.
    /// </summary>
    /// <remarks>Returns <see langword="null"/> if the connection fails, an error occurs during
    /// transmission, or the server does not send a valid reply.</remarks>
    /// <param name="message">The message to send to the server. Cannot be null.</param>
    /// <returns>A task that represents the asynchronous operation. The task result is the reply
    /// <see cref="Message"/> from the server, or <see langword="null"/> if the operation failed.</returns>
    public static async Task<Message?> SendMessageAndGetReplyAsync(Message message)
    {
        try
        {
            using (var pipeClient = new NamedPipeClientStream(PipeServerName, PipeName, PipeDirection.InOut))
            {
                await pipeClient.ConnectAsync(ConnectTimeoutMs);
                pipeClient.ReadMode = PipeTransmissionMode.Message;

                string json = message.Serialize();
                byte[] buffer = Encoding.UTF8.GetBytes(json);

                await pipeClient.WriteAsync(buffer, 0, buffer.Length);
                await pipeClient.FlushAsync();

                // Read reply from server
                using var ms = new MemoryStream();
                byte[] replyBuffer = new byte[4096];
                do
                {
                    int bytesRead = await pipeClient.ReadAsync(replyBuffer, 0, replyBuffer.Length);
                    if (bytesRead == 0)
                        break;
                    ms.Write(replyBuffer, 0, bytesRead);
                } while (!pipeClient.IsMessageComplete);

                if (ms.Length > 0)
                {
                    string replyJson = Encoding.UTF8.GetString(ms.ToArray());
                    return Message.Deserialize(replyJson);
                }

                return null;
            }
        }
        catch(Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error in PipeClient SendMessageAndGetReplyAsync: {ex}");
            return null;
        }
    }
}
