using System.Text.Json.Serialization;

namespace MoreFlyout.Comms;

[JsonSourceGenerationOptions(WriteIndented = true)]
[JsonSerializable(typeof(Message))]
internal partial class MessageJsonContext : JsonSerializerContext
{
}
