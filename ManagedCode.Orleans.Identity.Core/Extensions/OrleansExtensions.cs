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
    /// Parse roles from <typeparam>RequestContext</typeparam>
    /// </summary>
    /// <param name="filter">The incoming grain call filter instance used to access <typeparam>Request
    public static string[] GetRoles(this IIncomingGrainCallFilter filter)
    {
        return RequestContext.Get(ClaimTypes.Role) as string[] ?? [];
    }
}