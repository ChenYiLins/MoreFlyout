using System.IO.Pipes;
using System.Runtime.Versioning;
using System.Text;

namespace MoreFlyout.Comms;
    
/// <summary>
/// Provides a named pipe server for inter-process communication, enabling asynchronous message exchange between
/// processes on Windows platforms.
/// </summary>
/// <remarks>The pipe server listens for incoming client connections and raises events when messages are received
/// or errors occur. This class is intended for use on Windows operating systems, as indicated by the
/// SupportedOSPlatform attribute. Thread safety is not guaranteed for all members; callers should ensure appropriate
/// synchronization if accessing from multiple threads.</remarks>
[SupportedOSPlatform("windows")]
public class PipeServer
{
    private const string PipeName = "MoreFlyout.IPC";
    private NamedPipeServerStream? _pipeServer;
    private CancellationTokenSource? _cancellationTokenSource;

    /// <summary>
    /// When a message is received from the client, this event is triggered with the deserialized Message object.
    /// </summary>
    public event EventHandler<Message>? MessageReceived;

    /// <summary>
    /// When an error occurs in the pipe server, this event is triggered with a string describing the error.
    /// </summary>
    public event EventHandler<string>? ErrorOccurred;

    /// <summary>
    /// Starts the pipe server and listens for incoming client connections. This method runs indefinitely until the server is stopped.
    /// </summary>
    public async Task StartAsync(CancellationToken cancellationToken = default)
    {
        _cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

        try
        {
            while (!_cancellationTokenSource.Token.IsCancellationRequested)
            {
                _pipeServer = new NamedPipeServerStream(
                    PipeName,
                    PipeDirection.InOut,
                    NamedPipeServerStream.MaxAllowedServerInstances,
                    PipeTransmissionMode.Message);

                await _pipeServer.WaitForConnectionAsync(_cancellationTokenSource.Token);

                _ = HandleClientAsync(_pipeServer, _cancellationTokenSource.Token);
            }
        }
        catch (OperationCanceledException)
        {
            // Cancellation requested, exit gracefully
        }
        catch (Exception ex)
        {
            ErrorOccurred?.Invoke(this, $"Pipeline server error: {ex.Message}");
        }
    }

    /// <summary>
    /// Handles communication with a connected client. Reads data from the pipe, deserializes it into a Message object, and raises the MessageReceived event.
    /// </summary>
    private async Task HandleClientAsync(NamedPipeServerStream pipe, CancellationToken cancellationToken)
    {
        try
        {
            using (pipe)
            {
                byte[] buffer = new byte[4096];
                int bytesRead = await pipe.ReadAsync(buffer, 0, buffer.Length, cancellationToken);

                if (bytesRead > 0)
                {
                    string json = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                    var message = Message.Deserialize(json);

                    if (message != null)
                    {
                        MessageReceived?.Invoke(this, message);
                    }
                    else
                    {
                        ErrorOccurred?.Invoke(this, "Unable to deserialize message.");
                    }
                }
            }
        }
        catch (Exception ex)
        {
            ErrorOccurred?.Invoke(this, $"Handling client errors: {ex.Message}");
        }
    }

    /// <summary>
    /// Stops the pipe server by canceling any ongoing operations and disposing of the pipe server instance.
    /// </summary>
    public void Stop()
    {
        _cancellationTokenSource?.Cancel();
        _pipeServer?.Dispose();
        _cancellationTokenSource?.Dispose();
    }
}
