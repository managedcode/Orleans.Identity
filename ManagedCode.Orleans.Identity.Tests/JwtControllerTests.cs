using System.Net;
using System.Net.Http.Json;
using System.Security.Claims;
using FluentAssertions;
using ManagedCode.Orleans.Identity.Tests.Cluster;
using Xunit;
using Xunit.Abstractions;

namespace ManagedCode.Orleans.Identity.Tests;

[Collection(nameof(TestClusterApplication))]
public class JwtControllerTests(TestClusterApplication testApp, ITestOutputHelper outputHelper)
{
    private readonly ITestOutputHelper _outputHelper = outputHelper;

    private async Task<string> GetJwtToken(string username)
    {
        var client = testApp.CreateClient();
        var loginRequest = new { Username = username };
        var response = await client.PostAsJsonAsync("/auth/login", loginRequest);
        
        response.IsSuccessStatusCode.Should().BeTrue();
        var result = await response.Content.ReadFromJsonAsync<LoginResponse>();
        return result!.Token;
    }

    private HttpClient CreateAuthenticatedClient(string token)
    {
        var client = testApp.CreateClient();
        client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        return client;
    }

    [Fact]
    public async Task LoginWithJwt_ShouldReturnToken()
    {
        // Arrange
        var client = testApp.CreateClient();
        var loginRequest = new { Username = "user" };

        // Act
        var response = await client.PostAsJsonAsync("/auth/login", loginRequest);

        // Assert
        response.IsSuccessStatusCode.Should().BeTrue();
        var result = await response.Content.ReadFromJsonAsync<LoginResponse>();
        result.Should().NotBeNull();
        result!.Token.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task GetCurrentUser_WhenAuthenticated_ShouldReturnUserInfo()
    {
        // Arrange
        var token = await GetJwtToken("user");
        var client = CreateAuthenticatedClient(token);

        // Act
        var response = await client.GetAsync("/auth/me");

        // Assert
        response.IsSuccessStatusCode.Should().BeTrue();
        var userInfo = await response.Content.ReadFromJsonAsync<UserInfo>();
        userInfo.Should().NotBeNull();
        userInfo!.Username.Should().Be("user");
        userInfo.Roles.Should().Contain("user");
    }

    [Fact]
    public async Task GetUser_WhenAuthenticated_ShouldReturnPersonalizedMessage()
    {
        // Arrange
        var token = await GetJwtToken("user");
        var client = CreateAuthenticatedClient(token);

        // Act
        var response = await client.GetAsync("/userController");

        // Assert
        response.IsSuccessStatusCode.Should().BeTrue();
        var result = await response.Content.ReadAsStringAsync();
        result.Should().Contain("Hello, user!");
    }

    [Fact]
    public async Task GetUser_WhenNotAuthenticated_ShouldReturnUnauthorized()
    {
        // Arrange
        var client = testApp.CreateClient();

        // Act
        var response = await client.GetAsync("/userController");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task BanUser_WhenAdmin_ShouldReturnSuccess()
    {
        // Arrange
        var token = await GetJwtToken("admin");
        var client = CreateAuthenticatedClient(token);

        // Act
        var response = await client.GetAsync("/userController/ban");

        // Assert
        response.IsSuccessStatusCode.Should().BeTrue();
        var result = await response.Content.ReadAsStringAsync();
        result.Should().Contain("admin");
        result.Should().Contain("banned");
    }

    [Fact]
    public async Task BanUser_WhenNotAdmin_ShouldReturnForbidden()
    {
        // Arrange
        var token = await GetJwtToken("user");
        var client = CreateAuthenticatedClient(token);

        // Act
        var response = await client.GetAsync("/userController/ban");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GetPublicInfo_WhenNotAuthenticated_ShouldReturnSuccess()
    {
        // Arrange
        var client = testApp.CreateClient();

        // Act
        var response = await client.GetAsync("/userController/publicInfo");

        // Assert
        response.IsSuccessStatusCode.Should().BeTrue();
        var result = await response.Content.ReadAsStringAsync();
        result.Should().Contain("public information");
    }

    [Fact]
    public async Task ModifyUser_WhenModerator_ShouldReturnSuccess()
    {
        // Arrange
        var token = await GetJwtToken("moderator");
        var client = CreateAuthenticatedClient(token);

        // Act
        var response = await client.GetAsync("/userController/modify");

        // Assert
        response.IsSuccessStatusCode.Should().BeTrue();
        var result = await response.Content.ReadAsStringAsync();
        result.Should().Contain("moderator");
        result.Should().Contain("modified");
    }

    [Fact]
    public async Task ModifyUser_WhenNotModerator_ShouldReturnForbidden()
    {
        // Arrange
        var token = await GetJwtToken("user");
        var client = CreateAuthenticatedClient(token);

        // Act
        var response = await client.GetAsync("/userController/modify");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task AddToList_WhenAuthenticated_ShouldReturnSuccess()
    {
        // Arrange
        var token = await GetJwtToken("user");
        var client = CreateAuthenticatedClient(token);

        // Act
        var response = await client.GetAsync("/userController/addToList");

        // Assert
        response.IsSuccessStatusCode.Should().BeTrue();
        var result = await response.Content.ReadAsStringAsync();
        result.Should().Contain("user");
        result.Should().Contain("added to list");
    }
}

public class LoginResponse
{
    public string Token { get; set; } = string.Empty;
}

public class UserInfo
{
    public string UserId { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string[] Roles { get; set; } = Array.Empty<string>();
} 