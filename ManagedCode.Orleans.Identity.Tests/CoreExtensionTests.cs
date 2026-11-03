using System.Security.Claims;
using ManagedCode.Orleans.Identity.Core.Constants;
using ManagedCode.Orleans.Identity.Core.Extensions;
using ManagedCode.Orleans.Identity.Core.Serializations;
using Microsoft.Extensions.DependencyInjection;
using Orleans.Runtime;
using Orleans.Serialization;
using Shouldly;
using Xunit;

namespace ManagedCode.Orleans.Identity.Tests;

public class CoreExtensionTests
{
    private const string AuthenticationType = "test";
    private const string ClaimPropertyKey = "metadata-key";
    private const string ClaimPropertyValue = "metadata-value";
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
            var user = CreatePrincipal();
            RequestContext.Set(OrleansIdentityConstants.USER_CLAIMS, user);

            OrleansExtensions.GetRoles(null!).ShouldBe([RoleUser, RoleAdmin], ignoreOrder: true);
        }
        finally
        {
            RequestContext.Clear();
        }
    }

    [Fact]
    public void OrleansIdentityContext_RequiresAuthenticatedPrincipalAndUserId()
    {
        try
        {
            RequestContext.Clear();
            Should.Throw<UnauthorizedAccessException>(OrleansIdentityContext.RequireAuthenticatedPrincipal);
            RequestContext.Set(OrleansIdentityConstants.USER_CLAIMS, new ClaimsPrincipal(new ClaimsIdentity()));
            Should.Throw<UnauthorizedAccessException>(OrleansIdentityContext.RequireAuthenticatedPrincipal);
            RequestContext.Set(OrleansIdentityConstants.USER_CLAIMS,
                new ClaimsPrincipal(new ClaimsIdentity([], AuthenticationType)));
            Should.Throw<UnauthorizedAccessException>(OrleansIdentityContext.RequireAuthenticatedUserId);

            var principal = CreatePrincipal();
            RequestContext.Set(OrleansIdentityConstants.USER_CLAIMS, principal);
            OrleansIdentityContext.RequireAuthenticatedPrincipal().ShouldBeSameAs(principal);
            OrleansIdentityContext.RequireAuthenticatedUserId().ShouldBe(UserId);
        }
        finally
        {
            RequestContext.Clear();
        }
    }

    [Fact]
    public void UserDataExtensions_AddsRoleUsingIdentityRoleClaimType()
    {
        var identity = new ClaimsIdentity([], AuthenticationType, ClaimTypes.Name, "custom-role");
        identity.ParseRoles([RoleAdmin]);

        var principal = new ClaimsPrincipal(identity);
        principal.IsInRole(RoleAdmin).ShouldBeTrue();
        principal.GetRoles().ShouldBe([RoleAdmin]);
    }

    [Fact]
    public void RequireAuthenticatedUserId_DoesNotUseAnUnauthenticatedIdentity()
    {
        var authenticated = new ClaimsIdentity([], AuthenticationType);
        var unauthenticated = new ClaimsIdentity([new Claim(ClaimTypes.NameIdentifier, UserId)]);
        RequestContext.Set(OrleansIdentityConstants.USER_CLAIMS, new ClaimsPrincipal([authenticated, unauthenticated]));

        try
        {
            Should.Throw<UnauthorizedAccessException>(OrleansIdentityContext.RequireAuthenticatedUserId);
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
    public void ClaimConverter_RoundTripsClaimProperties()
    {
        var converter = new ClaimSurrogateConverter();
        var claim = new Claim(CustomClaimType, ClaimValue);
        claim.Properties[ClaimPropertyKey] = ClaimPropertyValue;

        var convertedClaim = converter.ConvertFromSurrogate(converter.ConvertToSurrogate(claim));

        convertedClaim.Properties[ClaimPropertyKey].ShouldBe(ClaimPropertyValue);
    }

    [Fact]
    public void ClaimsPrincipalWireRoundTrip_PreservesIdentityAndClaimOrder()
    {
        const string customName = "custom-name";
        const string customRole = "custom-role";
        var first = new ClaimsIdentity(
            [new Claim(customName, "first"), new Claim(customName, "second"), new Claim(customRole, RoleAdmin)],
            AuthenticationType, customName, customRole);
        first.Claims.First().Properties[ClaimPropertyKey] = ClaimPropertyValue;
        var second = new ClaimsIdentity([new Claim(ClaimTypes.Name, "other")]);
        var principal = new ClaimsPrincipal([first, second]);
        using var services = new ServiceCollection()
            .AddSerializer(builder => builder.AddAssembly(typeof(ClaimsPrincipalSurrogate).Assembly))
            .BuildServiceProvider();
        var serializer = services.GetRequiredService<Serializer>();

        var restored = serializer.Deserialize<ClaimsPrincipal>(serializer.SerializeToArray(principal));

        restored.ShouldNotBeNull();
        restored.Identities.Select(identity => identity.Name).ShouldBe(["first", "other"]);
        restored.Identities.First().Claims.Select(claim => claim.Value).ShouldBe(["first", "second", RoleAdmin]);
        restored.Identities.First().Claims.First().Properties[ClaimPropertyKey].ShouldBe(ClaimPropertyValue);
        restored.IsInRole(RoleAdmin).ShouldBeTrue();
        restored.GetRoles().ShouldBe([RoleAdmin]);
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
