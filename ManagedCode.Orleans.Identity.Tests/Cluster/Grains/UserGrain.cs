using ManagedCode.Orleans.Identity.Core.Extensions;
using ManagedCode.Orleans.Identity.Tests.Constants;
using Microsoft.AspNetCore.Authorization;
using Orleans;

namespace ManagedCode.Orleans.Identity.Tests.Cluster.Grains;

[Authorize]
public class UserGrain : Grain, IUserGrain
{
    private const string AuthenticationSchemeInfo = "Authentication scheme info";
    private const string InterfaceAdminInfoPrefix = "Interface admin info for ";
    private const string PolicyInfoPrefix = "Policy info for ";
    private const string UnknownUserName = "Unknown";

    [Authorize]
    public Task<string> GetUser()
    {
        var user = this.GetCurrentUser();
        var username = user.FindFirst(System.Security.Claims.ClaimTypes.Name)?.Value ?? UnknownUserName;
        return Task.FromResult($"Hello, {username}!");
    }

    [Authorize(Roles = TestRoles.ADMIN)]
    public Task<string> BanUser()
    {
        var user = this.GetCurrentUser();
        var username = user.FindFirst(System.Security.Claims.ClaimTypes.Name)?.Value ?? UnknownUserName;
        return Task.FromResult($"User {username} is banned");
    }

    [Authorize(Roles = TestRoles.ADMIN)]
    public Task<string> GetAdminInfo()
    {
        var user = this.GetCurrentUser();
        var username = user.FindFirst(System.Security.Claims.ClaimTypes.Name)?.Value ?? UnknownUserName;
        return Task.FromResult($"Admin info for {username}: You have admin privileges");
    }

    [AllowAnonymous]
    public Task<string> GetPublicInfo()
    {
        return Task.FromResult("This is public information - no authorization required");
    }

    [Authorize(Roles = TestRoles.MODERATOR)]
    public Task<string> ModifyUser()
    {
        var user = this.GetCurrentUser();
        var username = user.FindFirst(System.Security.Claims.ClaimTypes.Name)?.Value ?? UnknownUserName;
        return Task.FromResult($"User {username} has been modified");
    }

    public Task<string> AddToList()
    {
        var user = this.GetCurrentUser();
        var username = user.FindFirst(System.Security.Claims.ClaimTypes.Name)?.Value ?? UnknownUserName;
        return Task.FromResult($"User {username} added to list");
    }

    public Task<string> GetInterfaceAdminInfo()
    {
        var user = this.GetCurrentUser();
        var username = user.FindFirst(System.Security.Claims.ClaimTypes.Name)?.Value ?? UnknownUserName;
        return Task.FromResult($"{InterfaceAdminInfoPrefix}{username}");
    }

    public Task<string> GetPolicyInfo()
    {
        var user = this.GetCurrentUser();
        var username = user.FindFirst(System.Security.Claims.ClaimTypes.Name)?.Value ?? UnknownUserName;
        return Task.FromResult($"{PolicyInfoPrefix}{username}");
    }

    public Task<string> GetAuthenticationSchemeInfo()
    {
        return Task.FromResult(AuthenticationSchemeInfo);
    }
}
