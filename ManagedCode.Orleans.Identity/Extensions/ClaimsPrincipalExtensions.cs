using System.Linq;
using System.Security.Claims;
using ManagedCode.Orleans.Identity.Shared.Constants;

namespace ManagedCode.Orleans.Identity.Middlewares;

public static class ClaimsPrincipalExtensions
{
    public static string GetMobilePhone(this ClaimsPrincipal user)
    {
        return user.FindFirst(ClaimTypes.MobilePhone)?.Value ?? string.Empty;
    }

    public static string GetAccountId(this ClaimsPrincipal user)
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

    public static string GetSessionId(this ClaimsPrincipal user)
    {
        return user.FindFirst(ClaimTypes.Sid)?.Value ?? string.Empty;
    }

    public static string GetGrainId(this ClaimsPrincipal user)
    {
        return user.FindFirst(ClaimTypes.Actor)?.Value ?? string.Empty;
    }

    public static string[] GetRoles(this ClaimsPrincipal user)
    {
        return user.FindAll(ClaimTypes.Role).Select(s => s.Value).ToArray();
    }

    public static bool IsUnauthorizedClient(this ClaimsPrincipal user)
    {
        return user.GetGrainId() == OrleansIdentityConstants.AUTH_TOKEN;
    }
}