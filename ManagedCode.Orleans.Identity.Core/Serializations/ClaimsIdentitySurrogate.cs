using System.Collections.Generic;
using System.Security.Claims;
using Orleans;

namespace ManagedCode.Orleans.Identity.Core.Serializations;

// This is the surrogate which will act as a stand-in for the foreign type.
// Surrogates should use plain fields instead of properties for better performance.
[GenerateSerializer]
public struct ClaimsIdentitySurrogate(List<Claim>? claims, string? authenticationType, string? nameType, string? roleType)
{
    [Id(0)]
    public string? AuthenticationType { get; set; } = authenticationType;

    [Id(1)]
    public List<Claim>? Claims { get; set; } = claims;

    [Id(2)]
    public string? RoleType { get; set; } = roleType;

    [Id(3)]
    public string? NameType { get; set; } = nameType;
}

// This is a converter which converts between the surrogate and the foreign type.