using System.Text.Json.Serialization;
using Newtonsoft.Json.Converters;

namespace ManagedCode.Orleans.Identity.Server.Shared.Enums;

[JsonConverter(typeof(StringEnumConverter))]
public enum ResultStatus
{
    Unknown,
    PropertyWithThisKeyAlreadyExists,
    PropertyWithThisKeyDoesNotExist
}