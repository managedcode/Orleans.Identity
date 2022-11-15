using System.Text.Json.Serialization;

namespace ManagedCode.Orleans.Identity;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum Role
{
    Unknown,
    User,
    GlobalAdmin,
}