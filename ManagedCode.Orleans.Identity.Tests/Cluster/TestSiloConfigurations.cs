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
        siloBuilder.AddMemoryGrainStorage("sessionStore");

        siloBuilder.ConfigureServices(services =>
        {
            // services.AddGrpcOrleansScaling();
          //  services.AddApiOrleansScaling();
        });
    }
}