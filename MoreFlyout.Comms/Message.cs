using System.Text.Json;

namespace MoreFlyout.Comms;

public class Message
{
    /// <summary>
    /// Message type indicating the purpose of the message
    /// </summary>
    public MessageType Type { get; set; }

    /// <summary>
    /// Message content, can be used to carry additional data if needed
    /// </summary>
    public string? Content { get; set; }

    /// <summary>
    /// Message ID for tracking and correlation, default is a new GUID string
    /// </summary>
    public string MessageId { get; set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// Creation timestamp of the message, default is the current UTC time
    /// </summary>
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Serialize the message to a JSON string for transmission
    /// </summary>
    public string Serialize()
    {
        return JsonSerializer.Serialize(this, MessageJsonContext.Default.Message);
    }

    /// <summary>
    /// Deserialize a JSON string back into a Message object. Returns null if deserialization fails
    /// </summary>
    public static Message? Deserialize(string json)
    {
        try
        {
            return JsonSerializer.Deserialize(json, MessageJsonContext.Default.Message);
        }
        catch
        {
            return null;
        }
    }
}
