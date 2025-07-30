using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using Orleans;

namespace ManagedCode.Orleans.Identity.Core.Serializations;

[RegisterConverter]
public sealed class ClaimsPrincipalSurrogateConverter : IConverter<ClaimsPrincipal, ClaimsPrincipalSurrogate>
{
    public ClaimsPrincipal ConvertFromSurrogate(in ClaimsPrincipalSurrogate surrogate)
    {
        if (surrogate.Identities == null || surrogate.Identities.Count == 0)
        {
            return new ClaimsPrincipal();
        }

        return new ClaimsPrincipal(surrogate.Identities);
    }

    public ClaimsPrincipalSurrogate ConvertToSurrogate(in ClaimsPrincipal value)
    {
        var identities = value.Identities?.ToList();
        return new ClaimsPrincipalSurrogate(identities, value.Identity?.AuthenticationType);
    }
} 