using System.Diagnostics;
using MoreFlyout.Comms;
using MoreFlyout.Config;
using MoreFlyout.Service.Utils;

namespace MoreFlyout.Service.Services;

/// <summary>
/// Provides a service for managing inter-process communication using named pipes, enabling message-based interaction
/// between applications and the host service.
/// </summary>
/// <remarks>The service listens for messages from connected clients and processes commands such as enabling or
/// disabling auto-start, querying auto-start status, and controlling the Service lifecycle. It is designed to be started
/// asynchronously and can be stopped gracefully. Thread safety and error handling are managed internally. This class is
/// intended to be used as a long-running background service within an application.</remarks>
public class PipeCommService
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    private readonly PipeServer _pipeServer = new();
    private CancellationTokenSource? _cancellationTokenSource;

    public async Task StartAsync()
    {
        try
        {
            _cancellationTokenSource = new CancellationTokenSource();

            _pipeServer.MessageReceived += OnMessageReceived;
            _pipeServer.ErrorOccurred += OnErrorOccurred;
            _pipeServer.ReplyHandler = OnReplyRequested;

            Logger.Info("Pipeline communication service startup");

            // Start the pipe Service in a background task
            _ = _pipeServer.StartAsync(_cancellationTokenSource.Token);
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Failed to start pipeline communication service.");
        }
    }

    public void Stop()
    {
        try
        {
            _cancellationTokenSource?.Cancel();
            _pipeServer.Stop();
            Logger.Info("The pipeline communication service has stopped.");
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Failed to stop pipeline communication service.");
        }
    }

    private Message? OnReplyRequested(Message request)
    {
        switch (request.Type)
        {
            case MessageType.QueryAutoStart:
                string autoStartPath = CommonHelper.ExecutionPathService;
                return new Message { Type = MessageType.AutoStartResponse, Content = autoStartPath };

            case MessageType.QueryService:
                return new Message { Type = MessageType.ServiceResponse, Content = ResponseType.Ok };

            default:
                return null;
        }
    }

    private void OnMessageReceived(object? sender, Message message)
    {
        Logger.Info($"Received a message from the App: {message.Type}");

        try
        {
            switch (message.Type)
            {
                case MessageType.EnableAutoStart:
                    HandleEnableAutoStart();
                    break;

                case MessageType.DisableAutoStart:
                    HandleDisableAutoStart();
                    break;

                case MessageType.EnableTrayIcon:
                    HandleEnableTrayIcon();
                    break;

                case MessageType.DisableTrayIcon:
                    HandleDisableTrayIcon();
                    break;

                case MessageType.QueryAutoStart:
                    HandleQueryAutoStart();
                    break;

                case MessageType.RestartService:
                    HandleRestartService();
                    break;

                case MessageType.StopService:
                    HandleStopService();
                    break;

                case MessageType.QueryService:
                    break;

                default:
                    Logger.Warn($"Unknown message type: {message.Type}");
                    break;
            }
        }
        catch (Exception ex)
        {
            Logger.Error(ex, $"Processing message type {message.Type} error");
        }
    }

    private void OnErrorOccurred(object? sender, string errorMessage)
    {
        Logger.Error($"Pipeline communication error: {errorMessage}");
    }

    private void HandleEnableTrayIcon()
    {
        App.TrayIcon?.IsVisible = true;
        Logger.Info("Tray icon enabled.");
    }

    private void HandleDisableTrayIcon()
    {
        App.TrayIcon?.IsVisible = false;
        Logger.Info("Tray icon is disabled.");
    }

    private void HandleEnableAutoStart()
    {
        bool success = AutoStart.SetAutoStart(true);
        if (success)
        {
            ConfigManager.Instance.ServiceSettings.AutoStart = true;
            ConfigManager.Save();
            Logger.Info("Auto startup enabled.");
        }
        else
        {
            Logger.Error("Enabling auto startup failed.");
        }
    }

    private void HandleDisableAutoStart()
    {
        bool success = AutoStart.SetAutoStart(false);
        if (success)
        {
            ConfigManager.Instance.ServiceSettings.AutoStart = false;
            ConfigManager.Save();
            Logger.Info("Auto startup is disabled.");
        }
        else
        {
            Logger.Error("Disabling auto startup failed.");
        }
    }

    private void HandleQueryAutoStart()
    {
        Logger.Info($"Auto starting status query: {CommonHelper.ExecutionPathService}");
    }

    private void HandleRestartService()
    {
        App.ReleaseSingleInstanceMutex();
        Process.Start(new ProcessStartInfo(CommonHelper.ExecutionPathService) { UseShellExecute = false });
        App.Dispose();
        Application.Current.Exit();
        Logger.Info("Service restartup request has been processed.");
    }

    private void HandleStopService()
    {
        Logger.Info("Service is about to stop.");
        Application.Current.Exit();
    }
}
