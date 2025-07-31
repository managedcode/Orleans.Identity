using System.Net;
using System.Security.Claims;
using System.Text.Encodings.Web;
using ManagedCode.Orleans.Identity.Tests.Cluster;
using ManagedCode.Orleans.Identity.Tests.Constants;
using ManagedCode.Orleans.Identity.Tests.TestApp;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Orleans.TestingHost;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace ManagedCode.Orleans.Identity.Tests.Sessions;

[Collection(nameof(TestClusterApplication))]
public class SessionAuthTests(SessionAuthWebApplicationFactory factory, ITestOutputHelper outputHelper)
    : IClassFixture<SessionAuthWebApplicationFactory>
{
    private readonly ITestOutputHelper _outputHelper = outputHelper;

    #region Session Authentication - Basic Tests

    [Fact]
    public async Task SessionAuth_WhenUserAuthenticated_ShouldAccessProtectedEndpoint()
    {
        // Arrange
        var client = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                services.AddAuthentication("Test")
                    .AddScheme<TestAuthenticationSchemeOptions, TestAuthenticationHandler>("Test", options => { });
            });
        }).CreateClient();

        // Set authenticated user
        client.DefaultRequestHeaders.Add("Test-User", "testuser");
        client.DefaultRequestHeaders.Add("Test-Roles", "user");

        // Act
        var response = await client.GetAsync(TestControllerRoutes.USER_CONTROLLER_DEFAULT_ROUTE);
        var content = await response.Content.ReadAsStringAsync();

        // Assert
        response.IsSuccessStatusCode.ShouldBeTrue();
        content.ShouldContain("Hello, testuser!");
    }

    [Fact]
    public async Task SessionAuth_WhenUserNotAuthenticated_ShouldReturnUnauthorized()
    {
        // Arrange
        var client = factory.CreateClient();

        // Act
        var response = await client.GetAsync(TestControllerRoutes.USER_CONTROLLER_DEFAULT_ROUTE);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    #endregion

    #region Session Authentication - Role-based Tests

    [Fact]
    public async Task SessionAuth_WhenUserIsAdmin_ShouldAccessAdminEndpoint()
    {
        // Arrange
        var client = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                services.AddAuthentication("Test")
                    .AddScheme<TestAuthenticationSchemeOptions, TestAuthenticationHandler>("Test", options => { });
            });
        }).CreateClient();

        // Set authenticated admin user
        client.DefaultRequestHeaders.Add("Test-User", "admin");
        client.DefaultRequestHeaders.Add("Test-Roles", "user,admin");

        // Act
        var response = await client.GetAsync(TestControllerRoutes.USER_CONTROLLER_BAN);
        var content = await response.Content.ReadAsStringAsync();

        // Assert
        response.IsSuccessStatusCode.ShouldBeTrue();
        content.ShouldContain("User admin is banned");
    }

    [Fact]
    public async Task SessionAuth_WhenUserIsNotAdmin_ShouldReturnForbidden()
    {
        // Arrange
        var client = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                services.AddAuthentication("Test")
                    .AddScheme<TestAuthenticationSchemeOptions, TestAuthenticationHandler>("Test", options => { });
            });
        }).CreateClient();

        // Set authenticated user without admin role
        client.DefaultRequestHeaders.Add("Test-User", "user");
        client.DefaultRequestHeaders.Add("Test-Roles", "user");

        // Act
        var response = await client.GetAsync(TestControllerRoutes.USER_CONTROLLER_BAN);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task SessionAuth_WhenUserIsModerator_ShouldAccessModeratorEndpoint()
    {
        // Arrange
        var client = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                services.AddAuthentication("Test")
                    .AddScheme<TestAuthenticationSchemeOptions, TestAuthenticationHandler>("Test", options => { });
            });
        }).CreateClient();

        // Set authenticated moderator user
        client.DefaultRequestHeaders.Add("Test-User", "moderator");
        client.DefaultRequestHeaders.Add("Test-Roles", "user,moderator");

        // Act
        var response = await client.GetAsync(TestControllerRoutes.USER_CONTROLLER_MODIFY);
        var content = await response.Content.ReadAsStringAsync();

        // Assert
        response.IsSuccessStatusCode.ShouldBeTrue();
        content.ShouldContain("User moderator has been modified");
    }

    [Fact]
    public async Task SessionAuth_WhenUserIsNotModerator_ShouldReturnForbidden()
    {
        // Arrange
        var client = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                services.AddAuthentication("Test")
                    .AddScheme<TestAuthenticationSchemeOptions, TestAuthenticationHandler>("Test", options => { });
            });
        }).CreateClient();

        // Set authenticated user without moderator role
        client.DefaultRequestHeaders.Add("Test-User", "user");
        client.DefaultRequestHeaders.Add("Test-Roles", "user");

        // Act
        var response = await client.GetAsync(TestControllerRoutes.USER_CONTROLLER_MODIFY);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
    }

    #endregion

    #region Session Authentication - Public Endpoint Tests

    [Fact]
    public async Task SessionAuth_WhenUserNotAuthenticated_ShouldAccessPublicEndpoint()
    {
        // Arrange
        var client = factory.CreateClient();

        // Act
        var response = await client.GetAsync(TestControllerRoutes.USER_CONTROLLER_PUBLIC_INFO);
        var content = await response.Content.ReadAsStringAsync();

        // Assert
        response.IsSuccessStatusCode.ShouldBeTrue();
        content.ShouldContain("This is public information");
    }

    #endregion
}

// Test authentication handler for session-like authentication
public class TestAuthenticationHandler(IOptionsMonitor<TestAuthenticationSchemeOptions> options, ILoggerFactory logger, UrlEncoder encoder)
    : AuthenticationHandler<TestAuthenticationSchemeOptions>(options, logger, encoder)
{
    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var headers = Request.Headers;
        if (!headers.ContainsKey("Test-User"))
        {
            return Task.FromResult(AuthenticateResult.NoResult());
        }

        var username = headers["Test-User"].ToString();
        var roles = headers["Test-Roles"].ToString().Split(',');

        var claims = new List<Claim>
        {
            new(ClaimTypes.Name, username),
            new(ClaimTypes.NameIdentifier, username),
            new(ClaimTypes.Actor, username) // For grain ID
        };

        foreach (var role in roles)
        {
            if (!string.IsNullOrWhiteSpace(role))
            {
                claims.Add(new Claim(ClaimTypes.Role, role.Trim()));
            }
        }

        var identity = new ClaimsIdentity(claims, "Test");
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, "Test");

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}

public class TestAuthenticationSchemeOptions : AuthenticationSchemeOptions { }

// Custom WebApplicationFactory for session auth tests
public class SessionAuthWebApplicationFactory : WebApplicationFactory<HttpHostProgram>
{
    public TestCluster Cluster { get; }

    public SessionAuthWebApplicationFactory()
    {
        var builder = new TestClusterBuilder();
        builder.AddSiloBuilderConfigurator<TestSiloConfigurations>();
        builder.AddClientBuilderConfigurator<TestClientConfigurations>();
        Cluster = builder.Build();
        Cluster.Deploy();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");
        builder.ConfigureServices(services =>
        {
            // Add the Orleans cluster client
            services.AddSingleton(Cluster.Client);
        });
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        Cluster?.Dispose();
    }
}