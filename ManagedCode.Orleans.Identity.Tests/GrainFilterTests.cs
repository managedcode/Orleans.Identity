using FluentAssertions;
using ManagedCode.Orleans.Identity.Grains.Interfaces;
using ManagedCode.Orleans.Identity.Shared.Constants;
using ManagedCode.Orleans.Identity.Tests.Cluster;
using ManagedCode.Orleans.Identity.Tests.Constants;
using ManagedCode.Orleans.Identity.Tests.Helpers;
using Xunit;
using Xunit.Abstractions;

namespace ManagedCode.Orleans.Identity.Tests;

[Collection(nameof(TestClusterApplication))]
public class GrainFilterTests
{
    private readonly TestClusterApplication _testApp;
    private readonly ITestOutputHelper _outputHelper;
    
    public GrainFilterTests(TestClusterApplication testApp, ITestOutputHelper outputHelper)
    {
        _testApp = testApp;
        _outputHelper = outputHelper;
    }
    
    private async Task CreateSession(string sessionId, Dictionary<string, string> claims = null, bool replaceClaims = false)
    {
        var createSessionModel = SessionHelper.GetTestCreateSessionModel(sessionId, claims, replaceClaims);
        var sessionGrain = _testApp.Cluster.Client.GetGrain<ISessionGrain>(sessionId);
        await sessionGrain.CreateAsync(createSessionModel);
    }

    [Fact]
    public async Task SendRequestToAuthorizedGrain_WhenAuthorized_ReturnOk()
    {
        // Arrange
        var client = _testApp.CreateClient();
        var sessionId = Guid.NewGuid().ToString();
        await CreateSession(sessionId);
        client.DefaultRequestHeaders.Add(OrleansIdentityConstants.AUTH_TOKEN, sessionId);

        // Act
        var response = await client.GetAsync(TestControllerRoutes.USER_CONTROLLER_DEFAULT_ROUTE);

        // Assert
        response.IsSuccessStatusCode.Should().BeTrue();
    }
}