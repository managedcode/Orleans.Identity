using System.Net.Http.Json;
using ManagedCode.Orleans.Identity.Tests.Cluster;
using ManagedCode.Orleans.Identity.Tests.TestApp.Models;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.SignalR.Client;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace ManagedCode.Orleans.Identity.Tests.Jwt;

[Collection(nameof(TestClusterApplication))]
public class JwtSignalRTests
{
    private readonly ITestOutputHelper _outputHelper;
    private readonly TestClusterApplication _testApp;

    public JwtSignalRTests(TestClusterApplication testApp, ITestOutputHelper outputHelper)
    {
        _testApp = testApp;
        _outputHelper = outputHelper;
    }

    private async Task<string> GetJwtToken(string username)
    {
        var client = _testApp.CreateClient();
        var loginRequest = new { Username = username };
        var response = await client.PostAsJsonAsync("/auth/login", loginRequest);

        response.IsSuccessStatusCode.ShouldBeTrue();
        var result = await response.Content.ReadFromJsonAsync<LoginResponse>();
        return result!.Token;
    }

    private async Task<HubConnection> CreateSignalRConnection(string? token = null)
    {
        var connection = _testApp.CreateSignalRClient("TestAuthorizeHub", builder =>
        {
            builder.WithUrl($"{_testApp.Server.BaseAddress}TestAuthorizeHub", options =>
            {
                if (!string.IsNullOrEmpty(token))
                {
                    options.AccessTokenProvider = () => Task.FromResult<string?>(token);
                }
            });
        });

        await connection.StartAsync();
        return connection;
    }

    [Fact]
    public async Task SignalR_GetUserInfo_WhenAuthenticated_ShouldReturnPersonalizedMessage()
    {
        // Arrange
        var token = await GetJwtToken("user");
        var connection = await CreateSignalRConnection(token);

        try
        {
            // Act
            var result = await connection.InvokeAsync<string>("GetUserInfo");

            // Assert
            result.ShouldNotBeNullOrEmpty();
            result.ShouldContain("Hello, user!");
        }
        finally
        {
            await connection.DisposeAsync();
        }
    }

    [Fact]
    public async Task SignalR_GetPublicInfo_WhenAuthenticated_ShouldReturnPublicInfo()
    {
        // Arrange
        var token = await GetJwtToken("user");
        var connection = await CreateSignalRConnection(token);

        try
        {
            // Act
            var result = await connection.InvokeAsync<string>("GetPublicInfo");

            // Assert
            result.ShouldNotBeNullOrEmpty();
            result.ShouldContain("public information");
        }
        finally
        {
            await connection.DisposeAsync();
        }
    }
    
    [Fact]
    public async Task SignalR_GetPublicInfo_WhenNotAuthenticated_ShouldReturnPublicInfo()
    {
        // Arrange
        var connection = await CreateSignalRConnection();

        try
        {
            // Act
            var result = await connection.InvokeAsync<string>("GetPublicInfo");

            // Assert
            result.ShouldNotBeNullOrEmpty();
            result.ShouldContain("public information");
        }
        finally
        {
            await connection.DisposeAsync();
        }
    }

    [Fact]
    public async Task SignalR_GetAdminInfo_WhenAdmin_ShouldReturnSuccess()
    {
        // Arrange
        var token = await GetJwtToken("admin");
        var connection = await CreateSignalRConnection(token);

        try
        {
            // Act
            var result = await connection.InvokeAsync<string>("GetAdminInfo");

            // Assert
            result.ShouldNotBeNullOrEmpty();
            result.ShouldContain("admin");
            result.ShouldContain("admin privileges");
        }
        finally
        {
            await connection.DisposeAsync();
        }
    }

    [Fact]
    public async Task SignalR_GetAdminInfo_WhenNotAdmin_ShouldThrowException()
    {
        // Arrange
        var token = await GetJwtToken("user");
        var connection = await CreateSignalRConnection(token);

        try
        {
            // Act & Assert
            var exception = await Assert.ThrowsAsync<HubException>(() =>
                connection.InvokeAsync<string>("GetAdminInfo"));

            exception.Message.ShouldContain("Failed to invoke 'GetAdminInfo' because user is unauthorized");
        }
        finally
        {
            await connection.DisposeAsync();
        }
    }

    [Fact]
    public async Task SignalR_ConnectWithoutToken_ShouldFail()
    {
        // Arrange
        var connection = new HubConnectionBuilder()
            .WithUrl($"{_testApp.Server.BaseAddress}TestAuthorizeHub")
            .Build();

        // Act & Assert
        var exception = await Assert.ThrowsAsync<HttpRequestException>(() =>
            connection.StartAsync());

        await connection.DisposeAsync();
    }
}