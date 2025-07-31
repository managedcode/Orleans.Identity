using System;
using System.Security.Claims;
using System.Threading.Tasks;
using ManagedCode.Orleans.Identity.Core.Constants;
using Microsoft.AspNetCore.SignalR;
using Orleans.Runtime;

namespace ManagedCode.Orleans.Identity.Client.Filters;

public sealed class SignalRAuthorizationFilter : IHubFilter
{
    public ValueTask<object?> InvokeMethodAsync(
        HubInvocationContext invocationContext,
        Func<HubInvocationContext, ValueTask<object?>> next)
    {
        RequestContext.Set(OrleansIdentityConstants.USER_CLAIMS, invocationContext.Context.User!); // can be null
        return next(invocationContext);
    }

    public Task OnConnectedAsync(HubLifetimeContext context, Func<HubLifetimeContext, Task> next)
    {
        // Don't set RequestContext here as it won't persist to method calls
        return next(context);
    }

    public Task OnDisconnectedAsync(HubLifetimeContext context, Exception? exception, Func<HubLifetimeContext, Exception?, Task> next)
    {
        return next(context, exception);
    }
} 