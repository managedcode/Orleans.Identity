using System;
using System.Security.Claims;
using ManagedCode.Orleans.Identity.Core.Constants;
using Orleans.Runtime;

namespace ManagedCode.Orleans.Identity.Client.Filters;

internal readonly struct OrleansRequestContextScope : IDisposable
{
    private readonly object? previousUser;
    private readonly bool hasPreviousUser;

    public OrleansRequestContextScope(ClaimsPrincipal? user)
    {
        previousUser = RequestContext.Get(OrleansIdentityConstants.USER_CLAIMS);
        hasPreviousUser = previousUser is not null;

        SetUser(user);
    }

    public void Dispose()
    {
        if (hasPreviousUser)
        {
            RequestContext.Set(OrleansIdentityConstants.USER_CLAIMS, previousUser!);
            return;
        }

        RequestContext.Remove(OrleansIdentityConstants.USER_CLAIMS);
    }

    private static void SetUser(ClaimsPrincipal? user)
    {
        if (user is null)
        {
            RequestContext.Remove(OrleansIdentityConstants.USER_CLAIMS);
            return;
        }

        RequestContext.Set(OrleansIdentityConstants.USER_CLAIMS, user);
    }
}
