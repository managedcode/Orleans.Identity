using System.Security.Claims;
using ManagedCode.Orleans.Identity.Core.Extensions;
using ManagedCode.Orleans.Identity.Core.Serializations;
using Orleans.Runtime;
using Shouldly;
using Xunit;

namespace ManagedCode.Orleans.Identity.Tests;

public class CoreExtensionTests
{
    private const string AuthenticationType = "test";
    private const string ClaimValue = "claim-value";
    private const string CustomClaimType = "custom";
    private const string Delimiter = "|";
    private const string Email = "user@example.com";
    private const string GrainId = "grain-1";
    private const string MobilePhone = "+10000000000";
    private const string Phone = "+10000000001";
    private const string RoleAdmin = "admin";
    private const string RoleUser = "user";
    private const string UserId = "user-1";

    [Fact]
    public void ClaimsPrincipalExtensions_ReturnExpectedClaimValues()
    {
        var user = CreatePrincipal();

        user.GetMobilePhone().ShouldBe(MobilePhone);
        user.GetNameIdentifier().ShouldBe(UserId);
        user.GetEmail().ShouldBe(Email);
        user.GetPhone().ShouldBe(Phone);
        user.GetGrainId().ShouldBe(GrainId);
        user.GetRoles().ShouldBe([RoleUser, RoleAdmin], ignoreOrder: true);
    }

    [Fact]
    public void UserDataExtensions_AddRolesAndClaimsToIdentity()
    {
        var identity = new ClaimsIdentity(AuthenticationType);
        var roles = new HashSet<string> { RoleUser, RoleAdmin };
        var claims = new HashSet<string> { ClaimValue };

        roles.AsString(Delimiter).ShouldBe(string.Join(Delimiter, roles));

        identity.ParseRoles(roles);
        identity.ParseClaims(CustomClaimType, claims);

        identity.FindAll(ClaimTypes.Role).Select(claim => claim.Value).ShouldBe(roles, ignoreOrder: true);
        identity.FindFirst(CustomClaimType)!.Value.ShouldBe(ClaimValue);
    }

    [Fact]
    public void OrleansExtensions_ReadRolesFromRequestContext()
    {
        try
        {
            var roles = new[] { RoleUser, RoleAdmin };
            RequestContext.Set(ClaimTypes.Role, roles);

            OrleansExtensions.GetRoles(null!).ShouldBe(roles);
        }
        finally
        {
            RequestContext.Clear();
        }
    }

    [Fact]
    public void ClaimConverters_RoundTripClaimsPrincipal()
    {
        var claimConverter = new ClaimSurrogateConverter();
        var identityConverter = new ClaimsIdentitySurrogateConverter();
        var principalConverter = new ClaimsPrincipalSurrogateConverter();
        var principal = CreatePrincipal();
        var identity = (ClaimsIdentity)principal.Identity!;
        var claim = principal.Claims.First();

        var convertedClaim = claimConverter.ConvertFromSurrogate(claimConverter.ConvertToSurrogate(claim));
        var convertedIdentity = identityConverter.ConvertFromSurrogate(identityConverter.ConvertToSurrogate(identity));
        var convertedPrincipal = principalConverter.ConvertFromSurrogate(principalConverter.ConvertToSurrogate(principal));

        convertedClaim.Type.ShouldBe(claim.Type);
        convertedClaim.Value.ShouldBe(claim.Value);
        convertedIdentity.AuthenticationType.ShouldBe(AuthenticationType);
        convertedIdentity.Claims.Select(item => item.Type).ShouldContain(ClaimTypes.Email);
        convertedPrincipal.Identity!.AuthenticationType.ShouldBe(AuthenticationType);
    }

    [Fact]
    public void ClaimsPrincipalConverter_ReturnsEmptyPrincipalForEmptySurrogate()
    {
        var converter = new ClaimsPrincipalSurrogateConverter();

        var principal = converter.ConvertFromSurrogate(new ClaimsPrincipalSurrogate(null, null));

        principal.Identity.ShouldBeNull();
    }

    private static ClaimsPrincipal CreatePrincipal()
    {
        var identity = new ClaimsIdentity(
            [
                new Claim(ClaimTypes.MobilePhone, MobilePhone),
                new Claim(ClaimTypes.NameIdentifier, UserId),
                new Claim(ClaimTypes.Email, Email),
                new Claim(ClaimTypes.HomePhone, Phone),
                new Claim(ClaimTypes.Actor, GrainId),
                new Claim(ClaimTypes.Role, RoleUser),
                new Claim(ClaimTypes.Role, RoleAdmin),
            ],
            AuthenticationType
        );

        return new ClaimsPrincipal(identity);
    }
}
