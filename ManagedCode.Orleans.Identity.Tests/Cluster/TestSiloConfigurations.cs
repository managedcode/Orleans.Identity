using ManagedCode.Orleans.Identity.Shared.Constants;
using Orleans.Serialization;
using Orleans.TestingHost;

namespace ManagedCode.Orleans.Identity.Tests.Cluster;

public class TestSiloConfigurations : ISiloConfigurator
{
    public void Configure(ISiloBuilder siloBuilder)
    {
        siloBuilder.Services.AddSerializer(serializerBuilder =>
        {
            serializerBuilder.AddJsonSerializer();
        });

        // For test purpose
        siloBuilder.AddMemoryGrainStorage(OrleansIdentityConstants.SESSION_STORAGE_NAME);

        siloBuilder.ConfigureServices(services =>
        {
            // services.AddGrpcOrleansScaling();
          //  services.AddApiOrleansScaling();
        });
    }
}