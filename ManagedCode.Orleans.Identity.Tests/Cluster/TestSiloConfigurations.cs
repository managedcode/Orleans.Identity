using ManagedCode.Orleans.Identity.Core.Constants;
using ManagedCode.Orleans.Identity.Server.Extensions;
using ManagedCode.Orleans.Identity.Server.GrainCallFilter;
using Microsoft.Extensions.DependencyInjection;
using Orleans.Serialization;
using Orleans.TestingHost;

namespace ManagedCode.Orleans.Identity.Tests.Cluster;

public class TestSiloConfigurations : ISiloConfigurator
{
    public void Configure(ISiloBuilder siloBuilder)
    {
        // add OrleansIdentity
        siloBuilder.AddOrleansIdentity();


        // For test purpose
        siloBuilder.AddMemoryGrainStorage(OrleansIdentityConstants.SESSION_STORAGE);
        siloBuilder.UseInMemoryReminderService();

        siloBuilder.ConfigureServices(services =>
        {
            services.AddSingleton(TestSiloOptions.SessionOption);
        });
    }
}