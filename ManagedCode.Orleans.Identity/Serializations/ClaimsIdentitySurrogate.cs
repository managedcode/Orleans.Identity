using System.Collections.Generic;
using System.Security.Claims;
using Orleans;

namespace ManagedCode.Orleans.Identity.Serializations;

// This is the surrogate which will act as a stand-in for the foreign type.
// Surrogates should use plain fields instead of properties for better perfomance.
[GenerateSerializer]
public struct ClaimsIdentitySurrogate
{
    public ClaimsIdentitySurrogate(List<Claim>? claims, string? authenticationType, string? nameType, string? roleType)
    {
        AuthenticationType = authenticationType;
        Claims = claims;
        RoleType = roleType;
        NameType = nameType;
    }

    [Id(0)]
    public string? AuthenticationType { get; set; }

    [Id(1)]
    public List<Claim>? Claims { get; set; }

    [Id(2)]
    public string? RoleType { get; set; }

    [Id(3)]
    public string? NameType { get; set; }
}

// This is a converter which converts between the surrogate and the foreign type.