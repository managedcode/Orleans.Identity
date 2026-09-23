using System;
using System.Security.Claims;
using ManagedCode.Orleans.Identity.Core.Constants;
using Orleans.Runtime;

namespace ManagedCode.Orleans.Identity.Core.Extensions;

/// <summary>Reads the caller principal propagated by Orleans Identity without granting anonymous access.</summary>
public static class OrleansIdentityContext
{
    public static ClaimsPrincipal RequireAuthenticatedPrincipal()
    {
        if (RequestContext.Get(OrleansIdentityConstants.USER_CLAIMS) is ClaimsPrincipal
            { Identity: { IsAuthenticated: true } } principal)
        {
            return principal;
        }

        throw new UnauthorizedAccessException("Authenticated Orleans caller principal is required.");
    }

    public static string RequireAuthenticatedUserId()
    {
        var principal = RequireAuthenticatedPrincipal();
        var userId = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return string.IsNullOrWhiteSpace(userId)
            ? throw new UnauthorizedAccessException("Authenticated Orleans caller user id is required.")
            : userId;
    }
}
