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
    private const string PolicyInfoMessage = "Policy info";
    private const string PublicInformationMessage = "public information";
    private const string TestAuthenticationType = "Test";
    private const string TestUserName = "testuser";
    private const string UnsupportedAuthenticationSchemeMessage = "AuthenticationSchemes is not supported";

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

    [Fact]
    public async Task VerifyFilter_ClassAuthorization_WithoutUser_ShouldThrow()
    {
        var userGrain = GetUserGrainWithoutUserContext();

        try
        {
            var exception = await Should.ThrowAsync<UnauthorizedAccessException>(
                async () => await userGrain.AddToList()
            );
            exception.Message.ShouldContain(AccessDeniedMessage);
        }
        finally
        {
            RequestContext.Clear();
        }
    }

    [Fact]
    public async Task VerifyFilter_AllowAnonymous_WithoutUser_ShouldReturnResult()
    {
        var userGrain = GetUserGrainWithoutUserContext();

        try
        {
            var result = await userGrain.GetPublicInfo();
            result.ShouldContain(PublicInformationMessage);
        }
        finally
        {
            RequestContext.Clear();
        }
    }

    [Fact]
    public async Task VerifyFilter_PolicyAuthorization_WithoutRequiredClaim_ShouldThrow()
    {
        var userGrain = SetUserContextAndGetUserGrain(TestRoles.ADMIN);

        try
        {
            var exception = await Should.ThrowAsync<UnauthorizedAccessException>(
                async () => await userGrain.GetPolicyInfo()
            );
            exception.Message.ShouldContain(AccessDeniedMessage);
        }
        finally
        {
            RequestContext.Clear();
        }
    }

    [Fact]
    public async Task VerifyFilter_PolicyAuthorization_WithRequiredClaim_ShouldReturnResult()
    {
        var departmentClaim = new Claim(
            TestAuthorizationPolicies.DepartmentClaimType,
            TestAuthorizationPolicies.AdminDepartment
        );
        var userGrain = SetUserContextAndGetUserGrain(TestRoles.ADMIN, departmentClaim);

        try
        {
            var result = await userGrain.GetPolicyInfo();
            result.ShouldContain(PolicyInfoMessage);
        }
        finally
        {
            RequestContext.Clear();
        }
    }

    [Fact]
    public async Task VerifyFilter_AuthenticationSchemes_ShouldFailClosed()
    {
        var userGrain = SetUserContextAndGetUserGrain(TestRoles.ADMIN);

        try
        {
            var exception = await Should.ThrowAsync<InvalidOperationException>(
                async () => await userGrain.GetAuthenticationSchemeInfo()
            );
            exception.Message.ShouldContain(UnsupportedAuthenticationSchemeMessage);
        }
        finally
        {
            RequestContext.Clear();
        }
    }

    private IUserGrain SetUserContextAndGetUserGrain(string role, params Claim[] additionalClaims)
    {
        RequestContext.Set(OrleansIdentityConstants.USER_CLAIMS, CreatePrincipal(role, additionalClaims));
        return testApp.Cluster.Client.GetGrain<IUserGrain>(TestUserName);
    }

    private IUserGrain GetUserGrainWithoutUserContext()
    {
        RequestContext.Clear();
        return testApp.Cluster.Client.GetGrain<IUserGrain>(TestUserName);
    }

    private static ClaimsPrincipal CreatePrincipal(string role, params Claim[] additionalClaims)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.Name, TestUserName),
            new(ClaimTypes.NameIdentifier, TestUserName),
            new(ClaimTypes.Actor, TestUserName),
            new(ClaimTypes.Role, role),
        };
        claims.AddRange(additionalClaims);
        var identity = new ClaimsIdentity(claims, TestAuthenticationType);

        return new ClaimsPrincipal(identity);
    }
}
