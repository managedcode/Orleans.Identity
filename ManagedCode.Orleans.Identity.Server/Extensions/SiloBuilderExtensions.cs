using ManagedCode.Orleans.Identity.Server.GrainCallFilter;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Orleans;
using Orleans.Hosting;
using Orleans.Runtime;

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
        siloBuilder.AddIncomingGrainCallFilter<GrainAuthorizationIncomingFilter>();
        return siloBuilder;
    }
}