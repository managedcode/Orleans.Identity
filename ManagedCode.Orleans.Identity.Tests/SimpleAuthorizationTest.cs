using System.Net;
using System.Net.Http.Headers;
using ManagedCode.Orleans.Identity.Tests.Cluster;
using ManagedCode.Orleans.Identity.Tests.TestApp.Services;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace ManagedCode.Orleans.Identity.Tests;

[Collection(nameof(TestClusterApplication))]
public class SimpleAuthorizationTest
{
    private readonly ITestOutputHelper _outputHelper;
    private readonly TestClusterApplication _testApp;
    private readonly IJwtService _jwtService;

    public SimpleAuthorizationTest(TestClusterApplication testApp, ITestOutputHelper outputHelper)
    {
        _testApp = testApp;
        _outputHelper = outputHelper;
        _jwtService = new JwtService();
    }

    [Fact]
    public async Task User_Cannot_Access_Admin_Endpoint()
    {
        _outputHelper.WriteLine("=== Testing: User cannot access admin endpoint ===");
        
        // Create a client with user role only
        var client = _testApp.CreateClient();
        var token = _jwtService.GenerateToken("normaluser", "normaluser", new[] { "user" });
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        
        _outputHelper.WriteLine($"Created JWT token for user 'normaluser' with role 'user'");
        
        // Try to access admin endpoint
        var response = await client.GetAsync("/userController/ban");
        
        _outputHelper.WriteLine($"Response status: {response.StatusCode}");
        _outputHelper.WriteLine($"Response content: {await response.Content.ReadAsStringAsync()}");
        
        // Should get Forbidden (403)
        response.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
    }
    
    [Fact]
    public async Task Admin_Can_Access_Admin_Endpoint()
    {
        _outputHelper.WriteLine("=== Testing: Admin can access admin endpoint ===");
        
        // Create a client with admin role
        var client = _testApp.CreateClient();
        var token = _jwtService.GenerateToken("adminuser", "adminuser", new[] { "user", "admin" });
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        
        _outputHelper.WriteLine($"Created JWT token for user 'adminuser' with roles 'user, admin'");
        
        // Try to access admin endpoint
        var response = await client.GetAsync("/userController/ban");
        
        _outputHelper.WriteLine($"Response status: {response.StatusCode}");
        _outputHelper.WriteLine($"Response content: {await response.Content.ReadAsStringAsync()}");
        
        // Should succeed
        response.IsSuccessStatusCode.ShouldBeTrue();
        var content = await response.Content.ReadAsStringAsync();
        content.ShouldContain("adminuser");
        content.ShouldContain("banned");
    }
}