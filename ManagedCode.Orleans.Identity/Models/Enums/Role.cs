using System.Text.Json.Serialization;

namespace ManagedCode.Orleans.Identity.Models.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum Role
{
    Unknown,
    User,
    GlobalAdmin,
}