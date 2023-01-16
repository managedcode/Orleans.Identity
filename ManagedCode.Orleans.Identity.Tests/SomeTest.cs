using System.Net;
using FluentAssertions;
using ManagedCode.Orleans.Identity.Server.Grains.Interfaces;
using ManagedCode.Orleans.Identity.Server.Tests.Cluster;
using Microsoft.AspNetCore.SignalR.Client;
using Xunit;
using Xunit.Abstractions;

namespace ManagedCode.Orleans.Identity.Tests;

[Collection(nameof(TestClusterApplication))]
public class SomeTest
{
    private readonly ITestOutputHelper _outputHelper;
    private readonly TestClusterApplication _testApp;

    public SomeTest(TestClusterApplication testApp, ITestOutputHelper outputHelper)
    {
        _testApp = testApp;
        _outputHelper = outputHelper;
    }

    [Fact]
    public async Task OneRequest()
    {
        var gr = _testApp.Cluster.Client.GetGrain<ISessionGrain>("123");
        //await gr.AddClaimAsync(new Claim("1", "2"));
        var xx = await gr.GetSessionAsync();

        var anonymous = await _testApp.CreateClient().GetAsync("/anonymous");
        anonymous.StatusCode.Should().Be(HttpStatusCode.OK);

        var authorize = await _testApp.CreateClient().GetAsync("/authorize");
        var content = await authorize.Content.ReadAsStringAsync();
        authorize.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task OneSignalR()
    {
        var anonymousHub = _testApp.CreateSignalRClient(nameof(TestAnonymousHub));
        await anonymousHub.StartAsync();
        anonymousHub.State.Should().Be(HubConnectionState.Connected);

        var authorizeHub = _testApp.CreateSignalRClient(nameof(TestAuthorizeHub));
        await Assert.ThrowsAsync<HttpRequestException>(() => authorizeHub.StartAsync());
    }
}