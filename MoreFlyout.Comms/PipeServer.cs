using System.IO.Pipes;
using System.Text;

namespace MoreFlyout.Comms;

/// <summary>
/// 命名管道服务器，用于Server接收来自App的消息
/// </summary>
public class PipeServer
{
    private const string PipeName = "MoreFlyout.IPC";
    private NamedPipeServerStream? _pipeServer;
    private CancellationTokenSource? _cancellationTokenSource;

    /// <summary>
    /// 当收到消息时触发的事件
    /// </summary>
    public event EventHandler<Message>? MessageReceived;

    /// <summary>
    /// 当发生错误时触发的事件
    /// </summary>
    public event EventHandler<string>? ErrorOccurred;

    /// <summary>
    /// 启动管道服务器
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
            // 正常的取消操作
        }
        catch (Exception ex)
        {
            ErrorOccurred?.Invoke(this, $"管道服务器错误: {ex.Message}");
        }
    }

    /// <summary>
    /// 处理单个客户端连接
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
                        ErrorOccurred?.Invoke(this, "无法反序列化消息");
                    }
                }
            }
        }
        catch (Exception ex)
        {
            ErrorOccurred?.Invoke(this, $"处理客户端错误: {ex.Message}");
        }
    }

    /// <summary>
    /// 停止管道服务器
    /// </summary>
    public void Stop()
    {
        _cancellationTokenSource?.Cancel();
        _pipeServer?.Dispose();
        _cancellationTokenSource?.Dispose();
    }
}
