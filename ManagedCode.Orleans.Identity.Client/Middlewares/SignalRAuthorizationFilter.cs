using System;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.AspNetCore.SignalR;
using Orleans.Runtime;

namespace ManagedCode.Orleans.Identity.Client.Middlewares;

public class SignalRAuthorizationFilter : IHubFilter
{
    public async ValueTask<object?> InvokeMethodAsync(
        HubInvocationContext invocationContext,
        Func<HubInvocationContext, ValueTask<object?>> next)
    {
        if (invocationContext.Context.User?.Identity?.IsAuthenticated == true)
        {
            // Store user claims in Orleans RequestContext as serializable dictionary
            // Group claims by type to handle multiple values (like roles)
            var claims = invocationContext.Context.User.Claims
                .GroupBy(c => c.Type)
                .ToDictionary(g => g.Key, g => string.Join(",", g.Select(c => c.Value)));
            RequestContext.Set("UserClaims", claims);
        }

        return await next(invocationContext);
    }

    public Task OnConnectedAsync(HubLifetimeContext context, Func<HubLifetimeContext, Task> next)
    {
        if (context.Context.User?.Identity?.IsAuthenticated == true)
        {
            var claims = context.Context.User.Claims
                .GroupBy(c => c.Type)
                .ToDictionary(g => g.Key, g => string.Join(",", g.Select(c => c.Value)));
            RequestContext.Set("UserClaims", claims);
        }

        return next(context);
    }

    public Task OnDisconnectedAsync(HubLifetimeContext context, Exception? exception, Func<HubLifetimeContext, Exception?, Task> next)
    {
        return next(context, exception);
    }
} 