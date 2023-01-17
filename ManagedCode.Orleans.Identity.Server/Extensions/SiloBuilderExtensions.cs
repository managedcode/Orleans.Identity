using ManagedCode.Orleans.Identity.Server.GrainCallFilter;
using Orleans.Hosting;

namespace ManagedCode.Orleans.Identity.Server.Extensions;

public static class SiloBuilderExtensions
{
    public static ISiloBuilder AddOrleansIdentity(this ISiloBuilder siloBuilder)
    {
        siloBuilder.AddIncomingGrainCallFilter<GrainAuthorizationIncomingFilter>();
        return siloBuilder;
    }
}