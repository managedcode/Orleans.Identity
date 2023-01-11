using Newtonsoft.Json.Converters;
using System.Text.Json.Serialization;

namespace ManagedCode.Orleans.Identity.Shared.Enums;

[JsonConverter(typeof(StringEnumConverter))]
public enum ResultStatus
{
    Unknown,
    ThereIsSuchRoleAlready,
    SuchRoleDoesNotExist
}
