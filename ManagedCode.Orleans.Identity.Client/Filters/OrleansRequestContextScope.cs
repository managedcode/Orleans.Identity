using System;
using System.Security.Claims;
using ManagedCode.Orleans.Identity.Core.Constants;
using Orleans.Runtime;

namespace ManagedCode.Orleans.Identity.Client.Filters;

internal readonly struct OrleansRequestContextScope : IDisposable
{
    private readonly object? previousUser;

    public OrleansRequestContextScope(ClaimsPrincipal? user)
    {
        previousUser = RequestContext.Get(OrleansIdentityConstants.USER_CLAIMS);
        if (user is null || !ReferenceEquals(previousUser, user))
        {
            SetUser(user);
        }
    }

    public void Dispose()
    {
        if (previousUser is null || !ReferenceEquals(previousUser, RequestContext.Get(OrleansIdentityConstants.USER_CLAIMS)))
        {
            SetUser(previousUser);
        }
    }

    private static void SetUser(object? user)
    {
        if (user is null)
        {
            RequestContext.Remove(OrleansIdentityConstants.USER_CLAIMS);
            return;
        }

        RequestContext.Set(OrleansIdentityConstants.USER_CLAIMS, user);
    }
}
