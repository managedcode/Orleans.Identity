using System.Security.Claims;
using Orleans;

namespace ManagedCode.Orleans.Identity.Core.Serializations;

// This is the surrogate which will act as a stand-in for the foreign type.
// Surrogates should use plain fields instead of properties for better performance.
[GenerateSerializer]
public struct ClaimSurrogate(string type, string value, string valueType, string issuer, string originalIssuer)
{
    [Id(0)]
    public string Issuer { get; set; } = issuer;

    [Id(1)]
    public string OriginalIssuer { get; set; } = originalIssuer;

    [Id(2)]
    public string Type { get; set; } = type;

    [Id(3)]
    public string Value { get; set; } = value;

    [Id(4)]
    public string ValueType { get; set; } = valueType;
}

// This is a converter which converts between the surrogate and the foreign type.