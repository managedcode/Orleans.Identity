using Orleans.Serialization;
using Orleans.TestingHost;

namespace ManagedCode.Orleans.Identity.Tests.Cluster;

public class TestSiloConfigurations : ISiloConfigurator
{
    public void Configure(ISiloBuilder siloBuilder)
    {
        siloBuilder.Services.AddSerializer(serializerBuilder =>
        {
          //  serializerBuilder.AddJsonSerializer();
        });
        //siloBuilder.ConfigureApplicationParts(parts =>
        //{
        //    parts.AddFrameworkPart(typeof(IRequestTrackerGrain).Assembly);
        //    parts.AddFrameworkPart(typeof(RequestTrackerGrain).Assembly);
        //});

        siloBuilder.ConfigureServices(services =>
        {
            // services.AddGrpcOrleansScaling();
          //  services.AddApiOrleansScaling();
        });
    }
}