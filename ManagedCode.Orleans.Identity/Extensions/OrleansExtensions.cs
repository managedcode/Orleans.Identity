using System;
using System.Security.Claims;
using ManagedCode.Orleans.Identity.Shared.Constants;
using Orleans;
using Orleans.Runtime;

namespace ManagedCode.Orleans.Identity.Middlewares;

public static class OrleansExtensions
{
    public static void SetOrleansContext(this ClaimsPrincipal user)
    {
        // TODO: Check if user is autorized at all
        RequestContext.Set(OrleansIdentityConstants.SESSION_ID_CLAIM_NAME, user.GetRoles());
        RequestContext.Set(ClaimTypes.Role, user.GetRoles());
    }

    public static string[] GetOrleansContext(this IIncomingGrainCallFilter filter)
    {
        return RequestContext.Get(ClaimTypes.Role) as string[] ?? Array.Empty<string>();
    }
}