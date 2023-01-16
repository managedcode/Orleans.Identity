using System.Security.Claims;
using Orleans;

namespace ManagedCode.Orleans.Identity.Serializations;

// This is the surrogate which will act as a stand-in for the foreign type.
// Surrogates should use plain fields instead of properties for better perfomance.
[GenerateSerializer]
public struct ClaimSurrogate
{
    public ClaimSurrogate(string type, string value, string valueType, string issuer, string originalIssuer, ClaimsIdentity? subject)
    {
        Issuer = issuer;
        OriginalIssuer = originalIssuer;
        Subject = subject;
        Type = type;
        Value = value;
        ValueType = valueType;
    }

    [Id(0)]
    public string Issuer { get; set; }

    [Id(1)]
    public string OriginalIssuer { get; set; }

    [Id(2)]
    public ClaimsIdentity? Subject { get; set; }

    [Id(3)]
    public string Type { get; set; }

    [Id(4)]
    public string Value { get; set; }

    [Id(5)]
    public string ValueType { get; set; }
}

// This is a converter which converts between the surrogate and the foreign type.