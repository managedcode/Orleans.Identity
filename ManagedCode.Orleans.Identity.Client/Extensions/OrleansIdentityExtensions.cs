using ManagedCode.Orleans.Identity.Client.Filters;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

namespace ManagedCode.Orleans.Identity.Client.Extensions;

public static class OrleansIdentityExtensions
{
    public static IServiceCollection AddOrleansIdentity(this IServiceCollection services)
    {
        services.AddOptions();
        services.TryAddEnumerable(ServiceDescriptor.Singleton<IConfigureOptions<MvcOptions>, ConfigureIdentityFilters>());
        services.TryAddEnumerable(ServiceDescriptor.Singleton<IConfigureOptions<HubOptions>, ConfigureIdentityFilters>());
        return services;
    }

    private sealed class ConfigureIdentityFilters : IConfigureOptions<MvcOptions>, IConfigureOptions<HubOptions>
    {
        public void Configure(MvcOptions options) => options.Filters.Add<OrleansAuthorizationActionFilter>();

        public void Configure(HubOptions options) => options.AddFilter<SignalRAuthorizationFilter>();
    }
}
