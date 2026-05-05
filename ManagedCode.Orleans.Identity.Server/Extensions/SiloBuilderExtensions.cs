using ManagedCode.Orleans.Identity.Server.GrainCallFilter;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Orleans.Hosting;

namespace ManagedCode.Orleans.Identity.Server.Extensions;

public static class SiloBuilderExtensions
{
    /// <summary>
    /// Add incoming grain filter for authorization
    /// </summary>
    /// <param name="siloBuilder"></param>
    /// <returns></returns>
    public static ISiloBuilder AddOrleansIdentity(this ISiloBuilder siloBuilder)
    {
        ArgumentNullException.ThrowIfNull(siloBuilder);

        siloBuilder.Services.AddAuthorizationCore();
        siloBuilder.AddIncomingGrainCallFilter<GrainAuthorizationIncomingFilter>();
        return siloBuilder;
    }

    public static ISiloBuilder AddOrleansIdentity(this ISiloBuilder siloBuilder, Action<AuthorizationOptions> configureOptions)
    {
        ArgumentNullException.ThrowIfNull(siloBuilder);
        ArgumentNullException.ThrowIfNull(configureOptions);

        siloBuilder.Services.AddAuthorizationCore(configureOptions);
        siloBuilder.AddIncomingGrainCallFilter<GrainAuthorizationIncomingFilter>();
        return siloBuilder;
    }
}
