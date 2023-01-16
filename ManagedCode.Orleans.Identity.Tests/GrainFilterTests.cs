using System.Net;
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

    [Fact]
    public async Task SendRequestToAuthorizedGrain_WhenMethodWitoutAttributeAndUserIsAuthorized_ReturnOk()
    {
        // Arrange
        var client = _testApp.CreateClient();
        var sessionId = Guid.NewGuid().ToString();
        await CreateSession(sessionId);
        client.DefaultRequestHeaders.Add(OrleansIdentityConstants.AUTH_TOKEN, sessionId);

        // Act
        var response = await client.GetAsync(TestControllerRoutes.USER_CONTROLLER_ADD_TO_LIST);

        // Assert
        response.IsSuccessStatusCode.Should().BeTrue();
    }

    #endregion

    #region User unauthorized no roles required

    [Fact]
    public async Task SendRequestToUnauthorizedRoute_WhenGrainIsAuthorized_ReturnFail()
    {
        // Arrange
        var client = _testApp.CreateClient();
        
        // Act
        var response = await client.GetAsync(TestControllerRoutes.USER_CONTROLLER_ANONYMOUS_ROUTE);

        // Assert
        response.IsSuccessStatusCode.Should().BeFalse();
        response.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
    }

    [Fact]
    public async Task SendRequestToUnauthorizedRoute_WhenGrainsMethodIsNotAuthorized_ReturnOk()
    {
        // Arrange
        var client = _testApp.CreateClient();

        // Act
        var response = await client.GetAsync(TestControllerRoutes.USER_CONTROLLER_PUBLIC_INFO_ROUTE);

        // Assert
        response.IsSuccessStatusCode.Should().BeTrue();
    }

    [Fact]
    public async Task SendRequestToUnauthorizedRoute_WhenGrainMethodWithoutAttribute_ReturnFail()
    {
        // Arrange
        var client = _testApp.CreateClient();

        // Act
        var response = await client.GetAsync(TestControllerRoutes.USER_CONTROLLER_ADD_TO_LIST);

        // Assert
        response.IsSuccessStatusCode.Should().BeFalse();
        response.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
    }

    #endregion

    #region User authorized and has roles and roles are required

    [Fact]
    public async Task SendRequestToAutorizedGrain_WhenRoleIsRequiredAndUserAuthorized_ReturnOk()
    {
        // Arrange
        var client = _testApp.CreateClient();
        var sessionId = Guid.NewGuid().ToString();
        await CreateSession(sessionId);
        client.DefaultRequestHeaders.Add(OrleansIdentityConstants.AUTH_TOKEN, sessionId);

        // Act
        var response = await client.GetAsync(TestControllerRoutes.USER_CONTROLLER_BAN_ROUTE);

        // Assert
        response.IsSuccessStatusCode.Should().BeTrue();
    }

    #endregion

    #region User authorized and has no role and role is required

    [Fact]
    public async Task SendRequestToAutorizedGrain_WhenRoleIsRequiredAndUserDoesntHaveRole_ReturnFail()
    {
        // Arrange
        var client = _testApp.CreateClient();
        var sessionId = Guid.NewGuid().ToString();
        await CreateSession(sessionId);
        client.DefaultRequestHeaders.Add(OrleansIdentityConstants.AUTH_TOKEN, sessionId);

        // Act
        var response = await client.GetAsync(TestControllerRoutes.USER_CONTROLLER_MODIFY);

        // Assert
        response.IsSuccessStatusCode.Should().BeFalse();
    }

    #endregion

    #region Grain without authorization

    [Fact]
    public async Task SendRequestToUnauthorizedGrain_WhenUserUnauthorized_ReturnOk()
    {
        // Arrange
        var client = _testApp.CreateClient();

        // Act
        var response = await client.GetAsync(TestControllerRoutes.PUBLIC_CONTROLLER_DEFAULT_ROUTE);

        // Assert
        response.IsSuccessStatusCode.Should().BeTrue();
    }

    [Fact]
    public async Task SendRequestToUnauthorizedGrain_WhenUserAndMethodAuthorized_ReturnOk()
    {
        // Arrange
        var client = _testApp.CreateClient();
        var sessionId = Guid.NewGuid().ToString();
        await CreateSession(sessionId);
        client.DefaultRequestHeaders.Add(OrleansIdentityConstants.AUTH_TOKEN, sessionId);
        
        // Act
        var response = await client.GetAsync(TestControllerRoutes.PUBLIC_CONTROLLER_AUTH_METHOD_ROUTE);

        // Assert
        response.IsSuccessStatusCode.Should().BeTrue();
    }
    
    [Fact]
    public async Task SendRequestToUnauthorizedGrain_WhenUserUnauthorizedAndGrainMethodAuthorized_ReturnOk()
    {
        // Arrange
        var client = _testApp.CreateClient();

        // Act
        var response = await client.GetAsync(TestControllerRoutes.PUBLIC_CONTROLLER_AUTH_METHOD_ROUTE);

        // Assert
        response.IsSuccessStatusCode.Should().BeFalse();
        response.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
    }

    [Fact]
    public async Task SendRequestToUnauthorizedGrain_WhenMethodRequiresRoleAndUserHasRole_ReturnOk()
    {
        // Arrange
        var client = _testApp.CreateClient();
        var sessionId = Guid.NewGuid().ToString();
        await CreateSession(sessionId);
        client.DefaultRequestHeaders.Add(OrleansIdentityConstants.AUTH_TOKEN, sessionId);
        
        // Act
        var response = await client.GetAsync(TestControllerRoutes.PUBLIC_CONTROLLER_ADMIN_METHOD_ROUTE);

        // Assert
        response.IsSuccessStatusCode.Should().BeTrue();
    }

    [Fact]
    public async Task SendRequestToUnauthorizedGrain_WhenMethodRequiresRoleAndUserHasNoRole_ReturnFail()
    {
        // Arrange
        var client = _testApp.CreateClient();
        var sessionId = Guid.NewGuid().ToString();
        await CreateSession(sessionId);
        client.DefaultRequestHeaders.Add(OrleansIdentityConstants.AUTH_TOKEN, sessionId);
        
        // Act
        var response = await client.GetAsync(TestControllerRoutes.PUBLIC_CONTROLLER_MODERATOR_METHOD_ROUTE);

        // Assert
        response.IsSuccessStatusCode.Should().BeFalse();   
        response.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
    }

    #endregion
}