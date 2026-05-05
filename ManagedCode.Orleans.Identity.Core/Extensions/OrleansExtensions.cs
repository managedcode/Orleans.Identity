using System.Security.Claims;
using ManagedCode.Orleans.Identity.Core.Constants;
using Orleans;
using Orleans.Runtime;

namespace ManagedCode.Orleans.Identity.Core.Extensions;

public static class OrleansExtensions
{
    /// <summary>
    /// Gets roles stored in the Orleans request context.
    /// </summary>
    /// <param name="filter">The incoming grain call filter instance.</param>
    public static string[] GetRoles(this IIncomingGrainCallFilter filter)
    {
        var requestContext = RequestContext.Get(OrleansIdentityConstants.USER_CLAIMS);
        return requestContext is ClaimsPrincipal user ? user.GetRoles() : [];
    }
}
