using ManagedCode.Orleans.Identity.Server.Extensions;
using Orleans.TestingHost;

namespace ManagedCode.Orleans.Identity.Tests.Cluster;

public class TestSiloConfigurations : ISiloConfigurator
{
    public void Configure(ISiloBuilder siloBuilder)
    {
        // Add Orleans Identity server-side components
        siloBuilder.AddOrleansIdentity();

        // For test purpose - in-memory reminder service
        siloBuilder.UseInMemoryReminderService();
    }
}