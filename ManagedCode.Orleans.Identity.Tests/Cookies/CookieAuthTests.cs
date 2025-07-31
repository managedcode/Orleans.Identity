using System.Net;
using System.Net.Http.Json;
using ManagedCode.Orleans.Identity.Tests.Cluster;
using ManagedCode.Orleans.Identity.Tests.Constants;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace ManagedCode.Orleans.Identity.Tests.Cookies;

[Collection(nameof(TestClusterApplication))]
public class CookieAuthTests(TestClusterApplication testApp, ITestOutputHelper outputHelper)
    : IClassFixture<TestClusterApplication>
{
    private readonly ITestOutputHelper _outputHelper = outputHelper;

    #region Cookie Authentication - Basic Tests

    [Fact]
    public async Task CookieAuth_WhenUserAuthenticated_ShouldAccessProtectedEndpoint()
    {
        // Arrange
        var client = testApp.CreateClient();
        await LoginWithCookie(client, "testuser");

        // Act
        var response = await client.GetAsync(TestControllerRoutes.USER_CONTROLLER_DEFAULT_ROUTE);
        var content = await response.Content.ReadAsStringAsync();

        // Assert
        response.IsSuccessStatusCode.ShouldBeTrue();
        content.ShouldContain("Hello, testuser!");
    }

    [Fact]
    public async Task CookieAuth_WhenUserNotAuthenticated_ShouldReturnUnauthorized()
    {
        // Arrange
        var client = testApp.CreateClient();

        // Act
        var response = await client.GetAsync(TestControllerRoutes.USER_CONTROLLER_DEFAULT_ROUTE);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    #endregion

    #region Cookie Authentication - Role-based Tests

    [Fact]
    public async Task CookieAuth_WhenUserIsAdmin_ShouldAccessAdminEndpoint()
    {
        // Arrange
        var client = testApp.CreateClient();
        await LoginWithCookie(client, "admin");

        // Act
        var response = await client.GetAsync(TestControllerRoutes.USER_CONTROLLER_BAN);
        var content = await response.Content.ReadAsStringAsync();

        // Assert
        response.IsSuccessStatusCode.ShouldBeTrue();
        content.ShouldContain("User admin is banned");
    }

    [Fact]
    public async Task CookieAuth_WhenUserIsNotAdmin_ShouldReturnForbidden()
    {
        // Arrange
        var client = testApp.CreateClient();
        await LoginWithCookie(client, "user");

        // Act
        var response = await client.GetAsync(TestControllerRoutes.USER_CONTROLLER_BAN);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task CookieAuth_WhenUserIsModerator_ShouldAccessModeratorEndpoint()
    {
        // Arrange
        var client = testApp.CreateClient();
        await LoginWithCookie(client, "moderator");

        // Act
        var response = await client.GetAsync(TestControllerRoutes.USER_CONTROLLER_MODIFY);
        var content = await response.Content.ReadAsStringAsync();

        // Assert
        response.IsSuccessStatusCode.ShouldBeTrue();
        content.ShouldContain("User moderator has been modified");
    }

    [Fact]
    public async Task CookieAuth_WhenUserIsNotModerator_ShouldReturnForbidden()
    {
        // Arrange
        var client = testApp.CreateClient();
        await LoginWithCookie(client, "user");

        // Act
        var response = await client.GetAsync(TestControllerRoutes.USER_CONTROLLER_MODIFY);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
    }

    #endregion

    #region Cookie Authentication - Public Endpoint Tests

    [Fact]
    public async Task CookieAuth_WhenUserNotAuthenticated_ShouldAccessPublicEndpoint()
    {
        // Arrange
        var client = testApp.CreateClient();

        // Act
        var response = await client.GetAsync(TestControllerRoutes.USER_CONTROLLER_PUBLIC_INFO);
        var content = await response.Content.ReadAsStringAsync();

        // Assert
        response.IsSuccessStatusCode.ShouldBeTrue();
        content.ShouldContain("This is public information");
    }

    #endregion

    #region Cookie Authentication - Logout Tests

    [Fact]
    public async Task CookieAuth_WhenUserLogsOut_ShouldNotAccessProtectedEndpoint()
    {
        // Arrange
        var client = testApp.CreateClient();
        await LoginWithCookie(client, "testuser");

        // Verify user is authenticated
        var authResponse = await client.GetAsync(TestControllerRoutes.USER_CONTROLLER_DEFAULT_ROUTE);
        authResponse.IsSuccessStatusCode.ShouldBeTrue();

        // Act - Logout
        var logoutResponse = await client.PostAsync("/auth/logout", null);
        logoutResponse.IsSuccessStatusCode.ShouldBeTrue();

        // Act - Try to access protected endpoint
        var response = await client.GetAsync(TestControllerRoutes.USER_CONTROLLER_DEFAULT_ROUTE);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    #endregion

    #region Helper Methods

    private async Task LoginWithCookie(HttpClient client, string username)
    {
        var loginRequest = new LoginRequest { Username = username };
        var response = await client.PostAsJsonAsync("/auth/login-cookie", loginRequest);
        
        response.IsSuccessStatusCode.ShouldBeTrue();
    }

    #endregion
}

public class LoginRequest
{
    public string Username { get; set; } = string.Empty;
} 