using FluentAssertions;
using ManagedCode.Orleans.Identity.Grains.Interfaces;
using ManagedCode.Orleans.Identity.Models;
using ManagedCode.Orleans.Identity.Tests.Cluster;
using ManagedCode.Orleans.Identity.Tests.Constants;
using ManagedCode.Orleans.Identity.Tests.Helpers;
using Xunit;
using Xunit.Abstractions;

namespace ManagedCode.Orleans.Identity.Tests;

[Collection(nameof(TestClusterApplication))]
public class ControllerTests
{
    private readonly TestClusterApplication _testApp;
    private readonly ITestOutputHelper _outputHelper;

    public ControllerTests(TestClusterApplication testApp, ITestOutputHelper outputHelper)
    {
        _testApp = testApp;
        _outputHelper = outputHelper;
    }

    private async Task CreateSession(string sessionId)
    {
        var createSessionModel = SessionHelper.GetTestCreateSessionModel(sessionId);
        var sessionGrain = _testApp.Cluster.Client.GetGrain<ISessionGrain>(sessionId);
        await sessionGrain.CreateAsync(createSessionModel);
    }

    [Fact]
    public async Task SendRequestToUnAuthorizedRoute_ReturnOk()
    {
        // Arrange
        var sessionId = Guid.NewGuid().ToString();
        var client = _testApp.CreateClient();
        await CreateSession(sessionId);

        // Act
        var response = await client.GetAsync(TestControllerRoutes.ANONYMOUS_ROUTE);

        // Assert
        response.IsSuccessStatusCode.Should().BeTrue();
    }
}