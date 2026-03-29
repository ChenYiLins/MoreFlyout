namespace MoreFlyout.Comms;

/// <summary>
/// Specifies the types of messages used to control or query the auto-start and server operations.
/// </summary>
/// <remarks>This enumeration defines message types for enabling or disabling auto-start, querying auto-start
/// status, starting or stopping the server, and handling error responses. It is typically used in communication
/// protocols between components that manage server lifecycle and auto-start functionality.</remarks>
public enum MessageType
{
    /// <summary>
    /// Enable auto-start
    /// </summary>
    EnableAutoStart,

    /// <summary>
    /// Disable auto-start
    /// </summary>
    DisableAutoStart,

    EnableTrayIcon,

    DisableTrayIcon,

    /// <summary>
    /// Query auto-start status
    /// </summary>
    QueryAutoStart,

    /// <summary>
    /// Auto-start status response
    /// </summary>
    AutoStartResponse,

    /// <summary>
    /// Restart the server
    /// </summary>
    RestartServer,

    /// <summary>
    /// Stop the server
    /// </summary>
    StopServer,

    /// <summary>
    /// Represents a server that processes and responds to query requests.
    /// </summary>
    QueryServer,

    ServerResponse,

    /// <summary>
    /// Error response
    /// </summary>
    Error
}
