using System.Collections.Generic;
using System.Security.Claims;

namespace ManagedCode.Orleans.Identity.Extensions;

public static class UserDataExtensions
{
    public static string AsString(this HashSet<string> values, string delimiter = ";")
    {
        return string.Join(delimiter, values);
    }

    public static void ParseRoles(this ClaimsIdentity claimsIdentity, HashSet<string> roles)
    {
        foreach (var role in roles)
            claimsIdentity.AddClaim(new Claim(ClaimTypes.Role, role));
    }
}