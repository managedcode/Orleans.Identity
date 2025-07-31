using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Shouldly;
using ManagedCode.Orleans.Identity.Tests.Cluster;
using ManagedCode.Orleans.Identity.Tests.Constants;
using ManagedCode.Orleans.Identity.Tests.TestApp.Controllers;
using ManagedCode.Orleans.Identity.Tests.TestApp.Services;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using Xunit.Abstractions;

namespace ManagedCode.Orleans.Identity.Tests;

[Collection(nameof(TestClusterApplication))]
public class JwtGrainFilterTests
{
    private readonly ITestOutputHelper _outputHelper;
    private readonly TestClusterApplication _testApp;
    private readonly IJwtService _jwtService;

    public JwtGrainFilterTests(TestClusterApplication testApp, ITestOutputHelper outputHelper)
    {
        _testApp = testApp;
        _outputHelper = outputHelper;
        _jwtService = new JwtService();
    }

    private HttpClient CreateAuthenticatedClient(string username, params string[] roles)
    {
        var client = _testApp.CreateClient();
        var token = _jwtService.GenerateToken(username, username, roles);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return client;
    }

    #region User authorized no roles required

    [Fact]
    public async Task SendRequestToAuthorizedGrain_WhenAuthorized_ReturnOk()
    {
        // Arrange
        var client = CreateAuthenticatedClient("testuser", TestRoles.USER);

        // Act
        var response = await client.GetAsync(TestControllerRoutes.USER_CONTROLLER_DEFAULT_ROUTE);
        var content = await response.Content.ReadAsStringAsync();

        // Assert
        response.IsSuccessStatusCode.ShouldBeTrue();
        content.ShouldContain("Hello, testuser!");
    }

    [Fact]
    public async Task SendRequestToAuthorizedRoute_WhenRouteIsAnonymous_ReturnOk()
    {
        // Arrange
        var client = _testApp.CreateClient();

        // Act
        var response = await client.GetAsync(TestControllerRoutes.USER_CONTROLLER_PUBLIC_INFO);
        var content = await response.Content.ReadAsStringAsync();

        // Assert
        response.IsSuccessStatusCode.ShouldBeTrue();
        content.ShouldContain("This is public information");
    }

    [Fact]
    public async Task SendRequestToAuthorizedGrain_WhenMethodWithoutAttributeAndUserIsAuthorized_ReturnOk()
    {
        // Arrange
        var client = CreateAuthenticatedClient("testuser", TestRoles.USER);

        // Act
        var response = await client.GetAsync(TestControllerRoutes.USER_CONTROLLER_ADD_TO_LIST);
        var content = await response.Content.ReadAsStringAsync();

        // Assert
        response.IsSuccessStatusCode.ShouldBeTrue();
        content.ShouldContain("User testuser added to list");
    }

    #endregion

    #region User unauthorized no roles required

    [Fact]
    public async Task SendRequestToAuthorizedGrain_WhenUnauthorized_ReturnUnauthorized()
    {
        // Arrange
        var client = _testApp.CreateClient(); // No authentication

        // Act
        var response = await client.GetAsync(TestControllerRoutes.USER_CONTROLLER_DEFAULT_ROUTE);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task SendRequestToAuthorizedGrain_WhenTokenInvalid_ReturnUnauthorized()
    {
        // Arrange
        var client = _testApp.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "invalid_token");

        // Act
        var response = await client.GetAsync(TestControllerRoutes.USER_CONTROLLER_DEFAULT_ROUTE);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    #endregion

    #region Role-based authorization

    [Fact]
    public async Task SendRequestToAdminGrain_WhenUserIsAdmin_ReturnOk()
    {
        // Arrange
        var client = CreateAuthenticatedClient("admin", TestRoles.USER, TestRoles.ADMIN);

        // Act
        var response = await client.GetAsync(TestControllerRoutes.USER_CONTROLLER_BAN);
        var content = await response.Content.ReadAsStringAsync();

        // Assert
        response.IsSuccessStatusCode.ShouldBeTrue();
        content.ShouldContain("User admin is banned");
    }

    [Fact]
    public async Task SendRequestToAdminGrain_WhenUserIsNotAdmin_ReturnForbidden()
    {
        // Arrange
        var client = CreateAuthenticatedClient("user", TestRoles.USER);

        // Act
        var response = await client.GetAsync(TestControllerRoutes.USER_CONTROLLER_BAN);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task SendRequestToModeratorGrain_WhenUserIsModerator_ReturnOk()
    {
        // Arrange
        var client = CreateAuthenticatedClient("moderator", TestRoles.USER, TestRoles.MODERATOR);

        // Act
        var response = await client.GetAsync(TestControllerRoutes.USER_CONTROLLER_MODIFY);
        var content = await response.Content.ReadAsStringAsync();

        // Assert
        response.IsSuccessStatusCode.ShouldBeTrue();
        content.ShouldContain("User moderator has been modified");
    }

    [Fact]
    public async Task SendRequestToModeratorGrain_WhenUserIsNotModerator_ReturnForbidden()
    {
        // Arrange
        var client = CreateAuthenticatedClient("user", TestRoles.USER);

        // Act
        var response = await client.GetAsync(TestControllerRoutes.USER_CONTROLLER_MODIFY);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
    }

    #endregion

    #region Multiple roles

    [Fact]
    public async Task SendRequestWithMultipleRoles_WhenUserHasRequiredRole_ReturnOk()
    {
        // Arrange - User has both USER and ADMIN roles
        var client = CreateAuthenticatedClient("superuser", TestRoles.USER, TestRoles.ADMIN, TestRoles.MODERATOR);

        // Act - Try admin endpoint
        var adminResponse = await client.GetAsync(TestControllerRoutes.USER_CONTROLLER_BAN);
        var adminContent = await adminResponse.Content.ReadAsStringAsync();

        // Act - Try moderator endpoint
        var modResponse = await client.GetAsync(TestControllerRoutes.USER_CONTROLLER_MODIFY);
        var modContent = await modResponse.Content.ReadAsStringAsync();

        // Assert
        adminResponse.IsSuccessStatusCode.ShouldBeTrue();
        adminContent.ShouldContain("User superuser is banned");
        
        modResponse.IsSuccessStatusCode.ShouldBeTrue();
        modContent.ShouldContain("User superuser has been modified");
    }

    #endregion

    #region JWT Authentication endpoint tests

    [Fact]
    public async Task Login_WithValidUsername_ReturnsToken()
    {
        // Arrange
        var client = _testApp.CreateClient();
        var loginRequest = new LoginRequest { Username = "admin" };

        // Act
        var response = await client.PostAsJsonAsync("/auth/login", loginRequest);
        var content = await response.Content.ReadAsStringAsync();

        // Assert
        response.IsSuccessStatusCode.ShouldBeTrue();
        content.ShouldContain("token");
    }

    [Fact]
    public async Task GetCurrentUser_WithValidToken_ReturnsUserInfo()
    {
        // Arrange
        var client = CreateAuthenticatedClient("testuser", TestRoles.USER, TestRoles.ADMIN);

        // Act
        var response = await client.GetAsync("/auth/me");
        var content = await response.Content.ReadAsStringAsync();

        // Assert
        response.IsSuccessStatusCode.ShouldBeTrue();
        content.ShouldContain("testuser");
        content.ShouldContain(TestRoles.USER);
        content.ShouldContain(TestRoles.ADMIN);
    }

    #endregion
}