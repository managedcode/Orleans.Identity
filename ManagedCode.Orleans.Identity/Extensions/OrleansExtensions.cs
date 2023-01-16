using System;
using System.Security.Claims;
using Orleans;
using Orleans.Runtime;

namespace ManagedCode.Orleans.Identity.Middlewares;

public static class OrleansExtensions
{
    public static void SetOrleansContext(this ClaimsPrincipal user)
    {
        // TODO: Check if user is autorized at all
        RequestContext.Set(ClaimTypes.Role, user.GetRoles());
    }

    public static string[] GetOrleansContext(this IIncomingGrainCallFilter filter)
    {
        return RequestContext.Get(ClaimTypes.Role) as string[] ?? Array.Empty<string>();
    }
}