using System.Security.Claims;
using ManagedCode.Orleans.Identity.Core.Constants;
using ManagedCode.Orleans.Identity.Tests.Cluster;
using ManagedCode.Orleans.Identity.Tests.Cluster.Grains;
using ManagedCode.Orleans.Identity.Tests.Constants;
using Orleans.Runtime;
using Shouldly;
using Xunit;

namespace ManagedCode.Orleans.Identity.Tests;

[Collection(nameof(TestClusterApplication))]
public class VerifyFilterTest
{
    private const string AccessDeniedMessage = "Access denied";
    private const string TestAuthenticationType = "Test";
    private const string TestUserName = "testuser";

    private readonly TestClusterApplication testApp;

    public VerifyFilterTest(TestClusterApplication testApp)
    {
        this.testApp = testApp;
    }

    [Fact]
    public async Task VerifyFilter_DirectGrainCall_WithoutRole_ShouldThrow()
    {
        var userGrain = SetUserContextAndGetUserGrain(TestRoles.USER);

        try
        {
            var exception = await Should.ThrowAsync<UnauthorizedAccessException>(async () => await userGrain.BanUser());
            exception.Message.ShouldContain(AccessDeniedMessage);
        }
        finally
        {
            RequestContext.Clear();
        }
    }

    [Fact]
    public async Task VerifyFilter_InterfaceMethodAuthorization_WithoutRole_ShouldThrow()
    {
        var userGrain = SetUserContextAndGetUserGrain(TestRoles.USER);

        try
        {
            var exception = await Should.ThrowAsync<UnauthorizedAccessException>(
                async () => await userGrain.GetInterfaceAdminInfo()
            );
            exception.Message.ShouldContain(AccessDeniedMessage);
        }
        finally
        {
            RequestContext.Clear();
        }
    }

    private IUserGrain SetUserContextAndGetUserGrain(string role)
    {
        RequestContext.Set(OrleansIdentityConstants.USER_CLAIMS, CreatePrincipal(role));
        return testApp.Cluster.Client.GetGrain<IUserGrain>(TestUserName);
    }

    private static ClaimsPrincipal CreatePrincipal(string role)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.Name, TestUserName),
            new(ClaimTypes.NameIdentifier, TestUserName),
            new(ClaimTypes.Actor, TestUserName),
            new(ClaimTypes.Role, role),
        };
        var identity = new ClaimsIdentity(claims, TestAuthenticationType);

        return new ClaimsPrincipal(identity);
    }
}
