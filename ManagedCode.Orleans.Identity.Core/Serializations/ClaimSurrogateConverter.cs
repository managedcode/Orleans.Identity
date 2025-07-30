using System.Security.Claims;
using Orleans;

namespace ManagedCode.Orleans.Identity.Core.Serializations;

[RegisterConverter]
public sealed class ClaimSurrogateConverter : IConverter<Claim, ClaimSurrogate>
{
    public Claim ConvertFromSurrogate(in ClaimSurrogate surrogate)
    {
        return new Claim(surrogate.Type, surrogate.Value, surrogate.ValueType, surrogate.Issuer, surrogate.OriginalIssuer);
    }

    public ClaimSurrogate ConvertToSurrogate(in Claim value)
    {
        return new ClaimSurrogate(value.Type, value.Value, value.ValueType, value.Issuer, value.OriginalIssuer);
    }
}