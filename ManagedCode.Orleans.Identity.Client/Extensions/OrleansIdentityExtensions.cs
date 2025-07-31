using ManagedCode.Orleans.Identity.Client.Filters;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using Orleans;
using Orleans.Hosting;

namespace ManagedCode.Orleans.Identity.Client.Extensions;

public static class OrleansIdentityExtensions
{
    public static IServiceCollection AddOrleansIdentity(this IServiceCollection services)
    {
        // Add the action filter globally for all controllers
        services.Configure<MvcOptions>(options =>
        {
            options.Filters.Add<OrleansAuthorizationActionFilter>();
        });
        
        // Configure SignalR with the authorization filter
        services.Configure<HubOptions>(options =>
        {
            options.AddFilter<SignalRAuthorizationFilter>();
        });
        

        return services;
    }
} 