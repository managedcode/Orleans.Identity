using ManagedCode.Orleans.Identity.Client.Middlewares;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;

namespace ManagedCode.Orleans.Identity.Client.Extensions;

public static class OrleansIdentityExtensions
{
    public static IServiceCollection AddOrleansIdentity(this IServiceCollection services)
    {
        // Add middleware for storing claims in RequestContext
        services.AddScoped<OrleansContextMiddleware>();
        
        // Add SignalR filter for authorization
        services.AddSignalR(options =>
        {
            options.AddFilter<SignalRAuthorizationFilter>();
        });

        return services;
    }

    public static IApplicationBuilder UseOrleansIdentity(this IApplicationBuilder app)
    {
        // Add middleware to the pipeline
        app.UseMiddleware<OrleansContextMiddleware>();
        
        return app;
    }
} 