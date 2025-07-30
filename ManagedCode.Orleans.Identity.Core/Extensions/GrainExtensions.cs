using System;
using System.Security.Claims;
using Orleans;
using Orleans.Runtime;
using ManagedCode.Orleans.Identity.Core.Constants;

namespace ManagedCode.Orleans.Identity.Core.Extensions;

public static class GrainExtensions
{
    public static ClaimsPrincipal GetCurrentUser(this Grain grain)
    {
        var requestContext = RequestContext.Get(OrleansIdentityConstants.USER_CLAIMS);
        return requestContext as ClaimsPrincipal ?? new ClaimsPrincipal(new ClaimsIdentity());
    }
} 