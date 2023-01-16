using System.Security.Claims;
using FluentAssertions;
using ManagedCode.Orleans.Identity.Grains.Interfaces;
using ManagedCode.Orleans.Identity.Models;
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
    private readonly ITestOutputHelper _outputHelper;
    private readonly TestClusterApplication _testApp;
    
    private readonly Dictionary<string, HashSet<string>> claims = new()
    {
        { ClaimTypes.Role, new HashSet<string> { TestRoles.ADMIN } }
    };

    public GrainFilterTests(TestClusterApplication testApp, ITestOutputHelper outputHelper)
    {
        _testApp = testApp;
        _outputHelper = outputHelper;
    }

    private async Task CreateSession(string sessionId, Dictionary<string, HashSet<string>> claims = null, bool replaceClaims = false)
    {
        var createSessionModel = SessionHelper.GetTestCreateSessionModel(sessionId, claims, replaceClaims);
        var sessionGrain = _testApp.Cluster.Client.GetGrain<ISessionGrain>(sessionId);
        await sessionGrain.CreateAsync(createSessionModel);
    }
    
    private async Task CreateSession(string sessionId, CreateSessionModel createSessionModel)
    {
        var sessionGrain = _testApp.Cluster.Client.GetGrain<ISessionGrain>(sessionId);
        await sessionGrain.CreateAsync(createSessionModel);
    }

    #region User authorized no roles required

    // TODO: Incoming grain filter is working bad, works only when roles in attribute

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

    [Fact]
    public async Task SendRequestToAuthorizedRoute_WhenRouteIsUnauthorized_ReturnOk()
    {
        // Arrange
        var client = _testApp.CreateClient();
        var sessionId = Guid.NewGuid().ToString();
        await CreateSession(sessionId);
        client.DefaultRequestHeaders.Add(OrleansIdentityConstants.AUTH_TOKEN, sessionId);

        // Act
        var response = await client.GetAsync(TestControllerRoutes.USER_CONTROLLER_ANONYMOUS_ROUTE);

        // Assert
        response.IsSuccessStatusCode.Should().BeTrue();
    }

    #endregion

    #region User unauthorized no roles required

    [Fact]
    public async Task SendRequestToUnauthorizedRoute_WhenGrainIsAuthorized_ReturnOk()
    {
        // Arrange
        var client = _testApp.CreateClient();
        
        // Act
        var response = await client.GetAsync(TestControllerRoutes.USER_CONTROLLER_ANONYMOUS_ROUTE);

        // Assert
        response.IsSuccessStatusCode.Should().BeFalse();
    }

    #endregion
}