using System.Linq;
using System.Security.Claims;
using ManagedCode.Orleans.Identity.Core.Constants;

namespace ManagedCode.Orleans.Identity.Core.Extensions;

public static class ClaimsPrincipalExtensions
{
    public static string GetMobilePhone(this ClaimsPrincipal user)
    {
        return user.FindFirst(ClaimTypes.MobilePhone)?.Value ?? string.Empty;
    }

    public static string GetNameIdentifier(this ClaimsPrincipal user)
    {
        return user.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? string.Empty;
    }

    public static string GetEmail(this ClaimsPrincipal user)
    {
        return user.FindFirst(ClaimTypes.Email)?.Value ?? string.Empty;
    }

    public static string GetPhone(this ClaimsPrincipal user)
    {
        return user.FindFirst(ClaimTypes.HomePhone)?.Value ?? string.Empty;
    }
    
    public static string GetGrainId(this ClaimsPrincipal user)
    {
        return user.FindFirst(ClaimTypes.Actor)?.Value ?? string.Empty;
    }

    public static string[] GetRoles(this ClaimsPrincipal user)
    {
        return user.FindAll(ClaimTypes.Role).Select(s => s.Value).ToArray();
    }
}