using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Orleans.Runtime;
using ManagedCode.Orleans.Identity.Core.Constants;

namespace ManagedCode.Orleans.Identity.Client.Middlewares;

public class SignalRAuthorizationFilter : IHubFilter
{
    public async ValueTask<object?> InvokeMethodAsync(
        HubInvocationContext invocationContext,
        Func<HubInvocationContext, ValueTask<object?>> next)
    {
        if (invocationContext.Context.User?.Identity?.IsAuthenticated == true)
        {
            RequestContext.Set(OrleansIdentityConstants.USER_CLAIMS, invocationContext.Context.User);
        }

        return await next(invocationContext);
    }

    public Task OnConnectedAsync(HubLifetimeContext context, Func<HubLifetimeContext, Task> next)
    {
        if (context.Context.User?.Identity?.IsAuthenticated == true)
        {
            RequestContext.Set(OrleansIdentityConstants.USER_CLAIMS, context.Context.User);
        }

        return next(context);
    }

    public Task OnDisconnectedAsync(HubLifetimeContext context, Exception? exception, Func<HubLifetimeContext, Exception?, Task> next)
    {
        return next(context, exception);
    }
} 