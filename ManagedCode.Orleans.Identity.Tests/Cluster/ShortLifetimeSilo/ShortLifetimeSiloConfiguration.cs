using ManagedCode.Orleans.Identity.Core.Constants;
using ManagedCode.Orleans.Identity.Server.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Orleans.Configuration;
using Orleans.Serialization;
using Orleans.TestingHost;

namespace ManagedCode.Orleans.Identity.Tests.Cluster.ShortLifetimeSilo;

public class ShortLifetimeSiloConfiguration : ISiloConfigurator
{
    public void Configure(ISiloBuilder siloBuilder)
    {
        // add OrleansIdentity
        siloBuilder.AddOrleansIdentity();


        // For test purpose
        siloBuilder.AddMemoryGrainStorage(OrleansIdentityConstants.SESSION_STORAGE);
        siloBuilder.UseInMemoryReminderService();
        siloBuilder.Configure<GrainCollectionOptions>(options =>
        {
            options.CollectionAge = TimeSpan.FromMinutes(1).Add(TimeSpan.FromSeconds(30));
        });

        siloBuilder.ConfigureServices(services =>
        {
            services.AddSingleton(ShortLifetimeSiloOptions.SessionOption);
        });
    }
}