using System;
using System.Security.Claims;
using ManagedCode.Orleans.Identity.Constants;
using ManagedCode.Orleans.Identity.Extensions;
using Orleans;
using Orleans.Runtime;

namespace ManagedCode.Orleans.Identity.Extensions;

public static class OrleansExtensions
{
    public static void SetOrleansContext(this ClaimsPrincipal user)
    {
        RequestContext.Set(OrleansIdentityConstants.SESSION_ID_CLAIM_NAME, user.GetSessionId());
        RequestContext.Set(ClaimTypes.Role, user.GetRoles());
    }

    public static string[] GetOrleansContext(this IIncomingGrainCallFilter filter)
    {
        return RequestContext.Get(ClaimTypes.Role) as string[] ?? Array.Empty<string>();
    }

    public static string GetSessionId(this IIncomingGrainCallFilter filter)
    {
        return RequestContext.Get(OrleansIdentityConstants.SESSION_ID_CLAIM_NAME) as string ?? string.Empty;
    }
}