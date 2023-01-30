using ManagedCode.Orleans.Identity.Constants;
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
        siloBuilder.Services.AddSerializer(serializerBuilder => { serializerBuilder.AddJsonSerializer(); });

        // add OrleansIdentity
        siloBuilder.AddOrleansIdentity();


        // For test purpose
        siloBuilder.AddMemoryGrainStorage(OrleansIdentityConstants.SESSION_STORAGE_NAME);
        siloBuilder.AddMemoryGrainStorage(OrleansIdentityConstants.TOKEN_STORAGE_NAME);
        siloBuilder.UseInMemoryReminderService();

        siloBuilder.ConfigureServices(services =>
        {
            services.AddSingleton(TestSiloOptions.SessionOption);
            // services.AddGrpcOrleansScaling();
            //  services.AddApiOrleansScaling();
        });
    }
}