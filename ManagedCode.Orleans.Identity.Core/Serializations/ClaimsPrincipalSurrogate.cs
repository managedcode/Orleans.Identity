using System.Collections.Generic;
using System.Security.Claims;
using Orleans;

namespace ManagedCode.Orleans.Identity.Core.Serializations;

// This is the surrogate which will act as a stand-in for the foreign type.
// Surrogates should use plain fields instead of properties for better performance.
[GenerateSerializer]
public struct ClaimsPrincipalSurrogate(List<ClaimsIdentity>? identities, string? primaryIdentityType)
{
    [Id(0)]
    public List<ClaimsIdentity>? Identities { get; set; } = identities;

    [Id(1)]
    public string? PrimaryIdentityType { get; set; } = primaryIdentityType;
} 