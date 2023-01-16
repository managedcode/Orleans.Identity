using System.Text.Json.Serialization;

namespace ManagedCode.Orleans.Identity.Server.Shared.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum SessionStatus
{
    Active,
    Paused,
    Closed
}