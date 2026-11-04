using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using ManagedCode.Orleans.Identity.Tests.Cluster;
using ManagedCode.Orleans.Identity.Tests.Cluster.Grains;
using ManagedCode.Orleans.Identity.Tests.Constants;
using ManagedCode.Orleans.Identity.Tests.TestApp.Controllers;
using ManagedCode.Orleans.Identity.Tests.TestApp.Models;
using ManagedCode.Orleans.Identity.Tests.TestApp.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Net.Http.Headers;
using Shouldly;
using Xunit;
using Xunit.Abstractions;
using LoginRequest = ManagedCode.Orleans.Identity.Tests.Cookies.LoginRequest;

namespace ManagedCode.Orleans.Identity.Tests.Jwt;

[Collection(nameof(TestClusterApplication))]
public class JwtGrainFilterTests
{
    private readonly ITestOutputHelper _outputHelper;
    private readonly TestClusterApplication _testApp;

    public JwtGrainFilterTests(TestClusterApplication testApp, ITestOutputHelper outputHelper)
    {
        _testApp = testApp;
        _outputHelper = outputHelper;
    }

    private HttpClient CreateAuthenticatedClient(string username, params string[] roles)
    {
        var client = _testApp.CreateClient();
        using var scope = _testApp.Services.CreateScope();
        var token = scope.ServiceProvider.GetRequiredService<IJwtService>().GenerateToken(username, username, roles);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return client;
    }

    [Fact]
    public async Task JwtAndCookieExpiryUseTheInjectedClock()
    {
        var now = new DateTimeOffset(2000, 1, 1, 0, 0, 0, TimeSpan.Zero);
        var clock = new ManualUtcClock(now);
        using var host = _testApp.WithWebHostBuilder(builder => builder.ConfigureTestServices(
            services => services.AddSingleton<TimeProvider>(clock)));
        using var client = host.CreateClient(new() { HandleCookies = false });
        var login = new LoginRequest { Username = "admin" };

        using var jwtResponse = await client.PostAsJsonAsync("/auth/login", login);
        jwtResponse.EnsureSuccessStatusCode();
        var loginResponse = await jwtResponse.Content.ReadFromJsonAsync<LoginResponse>();
        loginResponse.ShouldNotBeNull();
        var token = new JwtSecurityTokenHandler().ReadJwtToken(loginResponse.Token);
        token.ValidTo.ShouldBe(now.AddHours(1).UtcDateTime);

        using var cookieResponse = await client.PostAsJsonAsync("/auth/login-cookie", login);
        cookieResponse.EnsureSuccessStatusCode();
        var cookie = SetCookieHeaderValue.Parse(cookieResponse.Headers.GetValues("Set-Cookie").Single());
        cookie.Expires.ShouldBe(now.AddHours(1));

        await AssertStatusAsync(HttpStatusCode.OK, HttpStatusCode.OK);
        clock.Advance(TimeSpan.FromHours(1) + TokenValidationParameters.DefaultClockSkew);
        await AssertStatusAsync(HttpStatusCode.OK, HttpStatusCode.Unauthorized);
        clock.Advance(TimeSpan.FromTicks(1));
        await AssertStatusAsync(HttpStatusCode.Unauthorized, HttpStatusCode.Unauthorized);

        async Task AssertStatusAsync(HttpStatusCode jwtStatus, HttpStatusCode cookieStatus)
        {
            using var jwtRequest = new HttpRequestMessage(HttpMethod.Get, "/auth/me");
            jwtRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", loginResponse.Token);
            using var jwtResult = await client.SendAsync(jwtRequest);
            jwtResult.StatusCode.ShouldBe(jwtStatus);
            if (jwtStatus == HttpStatusCode.Unauthorized)
            {
                var challenge = jwtResult.Headers.WwwAuthenticate.Single().Parameter;
                challenge.ShouldNotBeNull();
                challenge.ShouldContain("The token expired");
            }

            using var cookieRequest = new HttpRequestMessage(HttpMethod.Get, "/auth/me");
            cookieRequest.Headers.Add("Cookie", $"{cookie.Name}={cookie.Value}");
            using var cookieResult = await client.SendAsync(cookieRequest);
            cookieResult.StatusCode.ShouldBe(cookieStatus);
        }
    }

    private sealed class ManualUtcClock(DateTimeOffset now) : TimeProvider
    {
        private long utcTicks = now.UtcTicks;

        public override DateTimeOffset GetUtcNow() => new(Interlocked.Read(ref utcTicks), TimeSpan.Zero);

        public void Advance(TimeSpan interval) => Interlocked.Add(ref utcTicks, interval.Ticks);
    }

    [Fact]
    public async Task StreamResult_ForwardsPrincipalDuringEnumeration()
    {
        using var client = CreateAuthenticatedClient("admin", TestRoles.ADMIN);

        var values = await client.GetFromJsonAsync<string[]>("/userController/stream");

        values.ShouldBe(["http0", "http1", "http2"]);
    }

    [Fact]
    public async Task StreamResult_ForwardsPrincipalWhenAuthorizationFilterSuppliesResult()
    {
        using var host = _testApp.WithWebHostBuilder(builder => builder.ConfigureTestServices(services =>
            services.Configure<MvcOptions>(options => options.Filters.Add(new ShortCircuitStreamFilter()))));
        using var client = host.CreateClient();
        using var scope = host.Services.CreateScope();
        var token = scope.ServiceProvider.GetRequiredService<IJwtService>().GenerateToken("admin", "admin", [TestRoles.ADMIN]);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var values = await client.GetFromJsonAsync<string[]>("/userController/stream");

        values.ShouldBe(["short-circuit0", "short-circuit1", "short-circuit2"]);
    }

    private sealed class ShortCircuitStreamFilter : IAuthorizationFilter
    {
        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var client = context.HttpContext.RequestServices.GetRequiredService<IClusterClient>();
            var stream = client.GetGrain<IStreamingAuthorizationGrain>(Guid.NewGuid()).InterfaceProtected("short-circuit");
            context.Result = new ObjectResult(stream);
        }
    }

    [Fact]
    public async Task StreamResult_UsesReducedPrivilegesAfterActionReplacesPrincipal()
    {
        using var client = CreateAuthenticatedClient("admin", TestRoles.ADMIN);

        var values = await client.GetFromJsonAsync<string[]>("/userController/stream-downgraded");

        values.ShouldBe(["denied"]);
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