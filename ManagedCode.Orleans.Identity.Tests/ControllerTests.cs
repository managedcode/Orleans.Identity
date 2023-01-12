using FluentAssertions;
using ManagedCode.Orleans.Identity.Grains.Interfaces;
using ManagedCode.Orleans.Identity.Models;
using ManagedCode.Orleans.Identity.Shared.Constants;
using ManagedCode.Orleans.Identity.Tests.Cluster;
using ManagedCode.Orleans.Identity.Tests.Constants;
using ManagedCode.Orleans.Identity.Tests.Helpers;
using System.Net;
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
    public async Task SendRequestToUnauthorizedRoute_ReturnOk()
    {
        // Arrange
        var client = _testApp.CreateClient();

        // Act
        var response = await client.GetAsync(TestControllerRoutes.ANONYMOUS_ROUTE);

        // Assert
        response.IsSuccessStatusCode.Should().BeTrue();
    }

    [Fact]
    public async Task SendRequestToAuthorizedRoute_WhenNotAuthorized_ReturnUnauthorizedCode()
    {
        // Arrange
        var client = _testApp.CreateClient();

        // Act
        var response = await client.GetAsync(TestControllerRoutes.AUTHORIZE_ROUTE);

        // Assert
        response.IsSuccessStatusCode.Should().BeFalse();
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task SendRequestToAuthorizedRoute_WhenAuthorized_ReturnOk()
    {
        // Arrange
        var client = _testApp.CreateClient();
        var sessionId = Guid.NewGuid().ToString();
        await CreateSession(sessionId);
        client.DefaultRequestHeaders.Add(OrleansIdentityConstants.AUTH_TOKEN, sessionId);

        // Act
        var response = await client.GetAsync(TestControllerRoutes.AUTHORIZE_ROUTE);

        // Assert
        response.IsSuccessStatusCode.Should().BeTrue();
    }

    [Fact]
    public async Task SendRequestToAuthorizedRouteWithRole_WhenAuthorizedWithRole_ReturnOk()
    {
        // Arrange
        var client = _testApp.CreateClient();
        var sessionId = Guid.NewGuid().ToString();
        await CreateSession(sessionId);
        client.DefaultRequestHeaders.Add(OrleansIdentityConstants.AUTH_TOKEN, sessionId);

        // Act
        var response = await client.GetAsync(TestControllerRoutes.ADMIN_ROUTE);

        // Assert
        response.IsSuccessStatusCode.Should().BeTrue();
    }

    [Fact]
    public async Task SendRequestToAuthorizedRouteWithRole_WhenAuthorizedWithoutRole_ReturnUnauthorized()
    {
        // Arrange
        var client = _testApp.CreateClient();
        var sessionId = Guid.NewGuid().ToString();
        await CreateSession(sessionId);
        client.DefaultRequestHeaders.Add(OrleansIdentityConstants.AUTH_TOKEN, sessionId);

        // Act
        var response = await client.GetAsync(TestControllerRoutes.MODERATOR_ROUTE);

        // Assert
        response.IsSuccessStatusCode.Should().BeFalse();
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}