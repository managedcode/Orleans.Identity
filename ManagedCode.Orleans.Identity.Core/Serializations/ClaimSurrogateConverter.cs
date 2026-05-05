using System.Collections.Generic;
using System.Security.Claims;
using Orleans;

namespace ManagedCode.Orleans.Identity.Core.Serializations;

[RegisterConverter]
public sealed class ClaimSurrogateConverter : IConverter<Claim, ClaimSurrogate>
{
    public Claim ConvertFromSurrogate(in ClaimSurrogate surrogate)
    {
        var claim = new Claim(surrogate.Type, surrogate.Value, surrogate.ValueType, surrogate.Issuer, surrogate.OriginalIssuer);
        if (surrogate.Properties is null)
        {
            return claim;
        }

        foreach (var property in surrogate.Properties)
        {
            claim.Properties[property.Key] = property.Value;
        }

        return claim;
    }

    public ClaimSurrogate ConvertToSurrogate(in Claim value)
    {
        return new ClaimSurrogate(value.Type, value.Value, value.ValueType, value.Issuer, value.OriginalIssuer)
        {
            Properties = value.Properties.Count == 0 ? null : new Dictionary<string, string>(value.Properties),
        };
    }
}
