using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;

namespace ManagedCode.Orleans.Identity.Client.Filters;

public sealed class SignalRAuthorizationFilter : IHubFilter
{
    public ValueTask<object?> InvokeMethodAsync(
        HubInvocationContext invocationContext,
        Func<HubInvocationContext, ValueTask<object?>> next)
    {
        return InvokeMethodWithRequestContextAsync(invocationContext, next);
    }

    private static async ValueTask<object?> InvokeMethodWithRequestContextAsync(
        HubInvocationContext invocationContext,
        Func<HubInvocationContext, ValueTask<object?>> next)
    {
        using var requestContextScope = new OrleansRequestContextScope(invocationContext.Context.User);
        return await next(invocationContext);
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
