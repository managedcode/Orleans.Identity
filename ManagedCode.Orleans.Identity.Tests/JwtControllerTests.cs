using System.Net;
using System.Net.Http.Json;
using System.Security.Claims;
using Shouldly;
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
        
        response.IsSuccessStatusCode.ShouldBeTrue();
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
        response.IsSuccessStatusCode.ShouldBeTrue();
        var result = await response.Content.ReadFromJsonAsync<LoginResponse>();
        result.ShouldNotBeNull();
        result!.Token.ShouldNotBeNullOrEmpty();
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
        response.IsSuccessStatusCode.ShouldBeTrue();
        var userInfo = await response.Content.ReadFromJsonAsync<UserInfo>();
        userInfo.ShouldNotBeNull();
        userInfo!.Username.ShouldBe("user");
        userInfo.Roles.ShouldContain("user");
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
        response.IsSuccessStatusCode.ShouldBeTrue();
        var result = await response.Content.ReadAsStringAsync();
        result.ShouldContain("Hello, user!");
    }

    [Fact]
    public async Task GetUser_WhenNotAuthenticated_ShouldReturnUnauthorized()
    {
        // Arrange
        var client = testApp.CreateClient();

        // Act
        var response = await client.GetAsync("/userController");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
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
        response.IsSuccessStatusCode.ShouldBeTrue();
        var result = await response.Content.ReadAsStringAsync();
        result.ShouldContain("admin");
        result.ShouldContain("banned");
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
        response.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GetPublicInfo_WhenNotAuthenticated_ShouldReturnSuccess()
    {
        // Arrange
        var client = testApp.CreateClient();

        // Act
        var response = await client.GetAsync("/userController/publicInfo");

        // Assert
        response.IsSuccessStatusCode.ShouldBeTrue();
        var result = await response.Content.ReadAsStringAsync();
        result.ShouldContain("public information");
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
        response.IsSuccessStatusCode.ShouldBeTrue();
        var result = await response.Content.ReadAsStringAsync();
        result.ShouldContain("moderator");
        result.ShouldContain("modified");
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
        response.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
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
        response.IsSuccessStatusCode.ShouldBeTrue();
        var result = await response.Content.ReadAsStringAsync();
        result.ShouldContain("user");
        result.ShouldContain("added to list");
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