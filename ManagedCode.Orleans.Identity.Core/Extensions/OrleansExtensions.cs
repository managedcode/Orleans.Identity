using System;
using System.Security.Claims;
using ManagedCode.Orleans.Identity.Core.Constants;
using ManagedCode.Orleans.Identity.Core.Extensions;
using Orleans;
using Orleans.Runtime;

namespace ManagedCode.Orleans.Identity.Core.Extensions;

public static class OrleansExtensions
{
    /// <summary>
    /// Set roles and session id as claims to <c>ClaimsIdentity</c>
    /// </summary>
    /// <param name="user"><typeparam>ClaimsIdentity</typeparam> to set roles and session id</param>
    public static void SetOrleansContext(this ClaimsPrincipal user)
    {
        RequestContext.Set(OrleansIdentityConstants.SESSION_ID_CLAIM_NAME, user.GetSessionId());
        RequestContext.Set(ClaimTypes.Role, user.GetRoles());
        if (!string.IsNullOrEmpty(user.Identity?.Name))
        {
            RequestContext.Set(ClaimTypes.NameIdentifier, user.Identity.Name);
        }
    }

    /// <summary>
    /// Parse roles from <typeparam>RequestContext</typeparam>
    /// </summary>
    /// <param name="filter"></param>
    /// <returns>Roles as <typeparam>string[]</typeparam> </returns>
    public static string[] GetRoles(this IIncomingGrainCallFilter filter)
    {
        return RequestContext.Get(ClaimTypes.Role) as string[] ?? Array.Empty<string>();
    }

    /// <summary>
    /// Parse session id from <typeparam>RequestContext</typeparam>
    /// </summary>
    /// <param name="filter"></param>
    /// <returns>Session id as <typeparam>string</typeparam></returns>
    public static string GetSessionId(this IIncomingGrainCallFilter filter)
    {
        return RequestContext.Get(OrleansIdentityConstants.SESSION_ID_CLAIM_NAME) as string ?? string.Empty;
    }
}