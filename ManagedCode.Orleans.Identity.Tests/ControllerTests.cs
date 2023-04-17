using System.Net;
using System.Security.Claims;
using FluentAssertions;
using ManagedCode.Orleans.Identity.Core.Constants;
using ManagedCode.Orleans.Identity.Core.Interfaces;
using ManagedCode.Orleans.Identity.Core.Models;
using ManagedCode.Orleans.Identity.Tests.Cluster;
using ManagedCode.Orleans.Identity.Tests.Constants;
using ManagedCode.Orleans.Identity.Tests.Helpers;
using Orleans.Runtime;
using Xunit;
using Xunit.Abstractions;

namespace ManagedCode.Orleans.Identity.Tests;

[Collection(nameof(TestClusterApplication))]
public class ControllerTests
{
    private readonly ITestOutputHelper _outputHelper;
    private readonly TestClusterApplication _testApp;

    private Dictionary<string, string> claimsForAdminController = new()
    {
        { ClaimTypes.Role, "Moderator" },
        { ClaimTypes.Email, "test2@gmail.com" }
    };

    public ControllerTests(TestClusterApplication testApp, ITestOutputHelper outputHelper)
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

    #region Authorized route tests

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
    public async Task SendRequestToAuthorizedRouteWithRole_WhenAuthorizedWithoutRole_ReturnForbidden()
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

    [Fact]
    public async Task SendRequestToAuthorizedRouteWitheRoles_WhenAuthorizedWithRoles_ReturnOk()
    {
        // Arrange
        var client = _testApp.CreateClient();
        var sessionId = Guid.NewGuid().ToString();
        CreateSessionModel createSessionModel = new CreateSessionModel();
        createSessionModel.AddUserGrainId(SessionHelper.GetTestUserGrainId());
        createSessionModel.AddProperty(ClaimTypes.Role, new List<string> { TestRoles.ADMIN, TestRoles.MODERATOR });
        await CreateSession(sessionId, createSessionModel);
        client.DefaultRequestHeaders.Add(OrleansIdentityConstants.AUTH_TOKEN, sessionId);

        // Act
        var response = await client.GetAsync(TestControllerRoutes.ADMIN_CONTROLLER_EDIT_ADMINS);

        // Assert
        response.IsSuccessStatusCode.Should().BeTrue();
    }

    [Fact]
    public async Task SendRequestToAuthorizedRouteWitheRoles_WhenAuthorizedWithNotAllRoles_ReturnForbidden()
    {
        // Arrange
        var client = _testApp.CreateClient();
        var sessionId = Guid.NewGuid().ToString();
        await CreateSession(sessionId);
        client.DefaultRequestHeaders.Add(OrleansIdentityConstants.AUTH_TOKEN, sessionId);

        // Act
        var response = await client.GetAsync(TestControllerRoutes.ADMIN_CONTROLLER_EDIT_ADMINS);

        // Assert
        response.IsSuccessStatusCode.Should().BeFalse();
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    #endregion

    #region Authorized controller tests

    [Fact]
    public async Task SendRequestToAuthorizedController_WhenHasRole_ReturnOk()
    {
        // Arrange
        var client = _testApp.CreateClient();
        var sessionId = Guid.NewGuid().ToString();
        await CreateSession(sessionId);
        client.DefaultRequestHeaders.Add(OrleansIdentityConstants.AUTH_TOKEN, sessionId);

        // Act
        var response = await client.GetAsync(TestControllerRoutes.ADMIN_CONTROLLER_DEFAULT_ROUTE);

        // Assert
        response.IsSuccessStatusCode.Should().BeTrue();
    }

    [Fact]
    public async Task SendRequestToAuthorizedControllerToUnauthorizedRoute_WithoutRole_ReturnOk()
    {
        // Arrange
        var client = _testApp.CreateClient();
        var sessionId = Guid.NewGuid().ToString();
        CreateSessionModel createSessionModel = new CreateSessionModel();
        createSessionModel.AddUserGrainId(SessionHelper.GetTestUserGrainId());
        createSessionModel.AddProperty(ClaimTypes.Role, new List<string> { TestRoles.USER });
        await CreateSession(sessionId, createSessionModel);
        client.DefaultRequestHeaders.Add(OrleansIdentityConstants.AUTH_TOKEN, sessionId);

        // Act
        var response = await client.GetAsync(TestControllerRoutes.ADMIN_CONTROLLER_ADMINS_LIST);

        // Assert
        response.IsSuccessStatusCode.Should().BeTrue();
    }

    [Fact]
    public async Task SendRequestToAuthorizedControllerToUnauthorizedRoute_NotAuthorized_ReturnOk()
    {
        // Arrange
        var client = _testApp.CreateClient();

        // Act
        var response = await client.GetAsync(TestControllerRoutes.ADMIN_CONTROLLER_ADMINS_LIST);

        // Assert
        response.IsSuccessStatusCode.Should().BeTrue();
    }

    [Fact]
    public async Task SendRequestToAuthorizedControllerToAuthorizedRoute_WhenAutorized_ReturnForbidden()
    {
        // Arrange
        var client = _testApp.CreateClient();
        var sessionId = Guid.NewGuid().ToString();
        CreateSessionModel createSessionModel = new CreateSessionModel();
        createSessionModel.AddUserGrainId(SessionHelper.GetTestUserGrainId());
        createSessionModel.AddProperty(ClaimTypes.Role, new List<string> { TestRoles.USER });
        await CreateSession(sessionId, createSessionModel);
        client.DefaultRequestHeaders.Add(OrleansIdentityConstants.AUTH_TOKEN, sessionId);

        // Act
        var response = await client.GetAsync(TestControllerRoutes.ADMIN_CONTROLLER_ADMIN_GET_ADMIN);

        // Assert
        response.IsSuccessStatusCode.Should().BeFalse();
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task SendRequestToToAuthorizedControllerToAuthorizedRouteWithRole_WhenAutorizedWithRole_ReturnOk()
    {
        // Arrange
        var client = _testApp.CreateClient();
        var sessionId = Guid.NewGuid().ToString();
        CreateSessionModel createSessionModel = new CreateSessionModel();
        createSessionModel.AddUserGrainId(SessionHelper.GetTestUserGrainId());
        createSessionModel.AddProperty(ClaimTypes.Role, new List<string> { TestRoles.ADMIN, TestRoles.MODERATOR });
        await CreateSession(sessionId, createSessionModel);

        await CreateSession(sessionId, createSessionModel);
        client.DefaultRequestHeaders.Add(OrleansIdentityConstants.AUTH_TOKEN, sessionId);

        // Act
        var response = await client.GetAsync(TestControllerRoutes.ADMIN_CONTROLLER_ADMIN_GET_ADMIN);

        // Assert
        response.IsSuccessStatusCode.Should().BeTrue();
    }

    #endregion
}