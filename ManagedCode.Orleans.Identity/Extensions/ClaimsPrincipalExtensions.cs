using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Claims;
using System.Threading.Tasks;
using ManagedCode.Orleans.Identity.Models.Constants;
using Orleans;
using Orleans.Runtime;

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

public static class OrleansExtensions
{
    private const string Roles = "Roles";
    public static void SetOrleansContext(this ClaimsPrincipal user)
    {
        RequestContext.Set(Roles, user.GetRoles());
    }
    
    public static string[] GetOrleansContext(this IIncomingGrainCallFilter filter)
    {
        return RequestContext.Get(Roles) as string[] ?? Array.Empty<string>();
    }
    
}

