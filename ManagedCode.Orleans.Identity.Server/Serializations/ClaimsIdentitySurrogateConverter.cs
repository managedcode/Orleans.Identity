using System.Linq;
using System.Security.Claims;
using Orleans;

namespace ManagedCode.Orleans.Identity.Server.Serializations;

[RegisterConverter]
public sealed class ClaimsIdentitySurrogateConverter : IConverter<ClaimsIdentity, ClaimsIdentitySurrogate>
{
    public ClaimsIdentity ConvertFromSurrogate(in ClaimsIdentitySurrogate surrogate)
    {
        return new ClaimsIdentity(surrogate.Claims, surrogate.AuthenticationType, surrogate.NameType, surrogate.RoleType);
    }

    public ClaimsIdentitySurrogate ConvertToSurrogate(in ClaimsIdentity value)
    {
        return new ClaimsIdentitySurrogate(value.Claims.ToList(), value.AuthenticationType, value.NameClaimType, value.RoleClaimType);
    }
}