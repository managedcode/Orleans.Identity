using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Orleans.TestingHost;
using Xunit;

namespace ManagedCode.Orleans.Identity.Tests.Cluster;

[CollectionDefinition(nameof(TestClusterApplication))]
public class TestClusterApplication : WebApplicationFactory<HttpHostProgram>, ICollectionFixture<TestClusterApplication>
{
    public TestClusterApplication()
    {
        TestClusterBuilder builder = new TestClusterBuilder();
        builder.AddSiloBuilderConfigurator<TestSiloConfigurations>();
        builder.AddClientBuilderConfigurator<TestClientConfigurations>();
        Cluster = builder.Build();
        Cluster.Deploy();
    }

    public TestCluster Cluster { get; }

    protected override IHost CreateHost(IHostBuilder builder)
    {
        if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")))
        {
            Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Development");
        }

        builder.ConfigureServices(s =>
        {
            s.AddSingleton(Cluster.Client);
        });
        return base.CreateHost(builder);
    }

    public HubConnection CreateSignalRClient(string hubUrl, Action<HubConnectionBuilder>? configure = null)
    {
        HubConnectionBuilder builder = new HubConnectionBuilder();
        configure?.Invoke(builder);
        return builder.WithUrl(new Uri(Server.BaseAddress, hubUrl), o => o.HttpMessageHandlerFactory = _ => Server.CreateHandler())
            .Build();
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        Cluster.Dispose();
    }

    public override async ValueTask DisposeAsync()
    {
        await base.DisposeAsync();
        await Cluster.DisposeAsync();
    }
}