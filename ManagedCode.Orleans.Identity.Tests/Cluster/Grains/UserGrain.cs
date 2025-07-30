using ManagedCode.Orleans.Identity.Core.Extensions;
using ManagedCode.Orleans.Identity.Tests.Constants;
using Microsoft.AspNetCore.Authorization;

namespace ManagedCode.Orleans.Identity.Tests.Cluster.Grains;

[Authorize]
public class UserGrain : Grain, IUserGrain
{
    [Authorize]
    public Task<string> GetUser()
    {
        var user = this.GetCurrentUser();
        var username = user.FindFirst(System.Security.Claims.ClaimTypes.Name)?.Value ?? "Unknown";
        return Task.FromResult($"Hello, {username}!");
    }

    [Authorize(Roles = TestRoles.ADMIN)]
    public Task<string> BanUser()
    {
        var user = this.GetCurrentUser();
        var username = user.FindFirst(System.Security.Claims.ClaimTypes.Name)?.Value ?? "Unknown";
        return Task.FromResult($"User {username} is banned");
    }

    [Authorize(Roles = TestRoles.ADMIN)]
    public Task<string> GetAdminInfo()
    {
        var user = this.GetCurrentUser();
        var username = user.FindFirst(System.Security.Claims.ClaimTypes.Name)?.Value ?? "Unknown";
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
        var username = user.FindFirst(System.Security.Claims.ClaimTypes.Name)?.Value ?? "Unknown";
        return Task.FromResult($"User {username} has been modified");
    }

    public Task<string> AddToList()
    {
        var user = this.GetCurrentUser();
        var username = user.FindFirst(System.Security.Claims.ClaimTypes.Name)?.Value ?? "Unknown";
        return Task.FromResult($"User {username} added to list");
    }
}