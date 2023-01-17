using System.Collections.Generic;
using System.Security.Claims;

namespace ManagedCode.Orleans.Identity.Extensions;

public static class UserDataExtensions
{
    public static string AsString(this HashSet<string> values, string delimiter = ";")
    {
        return string.Join(delimiter, values);
    }

    /// <summary>
    /// Add roles from Hashset to <c>ClaimsIdentity</c>
    /// </summary>
    /// <param name="claimsIdentity"><c>ClaimsIdentity</c> to add roles</param>
    /// <param name="roles">String representation of roles to add</param>
    public static void ParseRoles(this ClaimsIdentity claimsIdentity, HashSet<string> roles)
    {
        foreach (var role in roles)
            claimsIdentity.AddClaim(new Claim(ClaimTypes.Role, role));
    }

    /// <summary>
    /// Add claims to <c>ClaimsIdentity</c>
    /// </summary>
    /// <param name="claimsIdentity"><c>ClaimsIdentity</c> to add claims</param>
    /// <param name="key">Key associated with claims</param>
    /// <param name="claims">Claims in string representation</param>
    public static void ParseClaims(this ClaimsIdentity claimsIdentity, string key, HashSet<string> claims)
    {
        foreach (var claim in claims)
            claimsIdentity.AddClaim(new Claim(key, claim));
    }
}