using ManagedCode.Orleans.Identity.Server.GrainCallFilter;
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
        siloBuilder.AddIncomingGrainCallFilter<GrainAuthorizationIncomingFilter>();
        return siloBuilder;
    }
}