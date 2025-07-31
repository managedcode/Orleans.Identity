using System.Linq;
using ManagedCode.Orleans.Identity.Core.Extensions;
using ManagedCode.Orleans.Identity.Tests.Constants;
using Microsoft.AspNetCore.Authorization;
using Orleans;
using ManagedCode.Orleans.Identity.Server.GrainCallFilter;

namespace ManagedCode.Orleans.Identity.Tests.Cluster.Grains;

[Authorize]
public class UserGrain : Grain, IUserGrain
{
    // Manual authorization check until grain filters work in Orleans 9
    private void CheckAuthorization(params string[] requiredRoles)
    {
        var user = this.GetCurrentUser();
        
        if (user == null || user.Identity?.IsAuthenticated != true)
        {
            throw new UnauthorizedAccessException("Access denied. User is not authenticated.");
        }
        
        if (requiredRoles.Length > 0)
        {
            var userRoles = user.FindAll(System.Security.Claims.ClaimTypes.Role).Select(c => c.Value).ToHashSet();
            if (!requiredRoles.Any(role => userRoles.Contains(role)))
            {
                throw new UnauthorizedAccessException("Access denied. User does not have required roles.");
            }
        }
    }
    
    [Authorize]
    public Task<string> GetUser()
    {
        CheckAuthorization(); // Manual check
        var user = this.GetCurrentUser();
        var username = user.FindFirst(System.Security.Claims.ClaimTypes.Name)?.Value ?? "Unknown";
        return Task.FromResult($"Hello, {username}!");
    }

    [Authorize(Roles = TestRoles.ADMIN)]
    public Task<string> BanUser()
    {
        CheckAuthorization(TestRoles.ADMIN); // Manual check for admin role
        var user = this.GetCurrentUser();
        var username = user.FindFirst(System.Security.Claims.ClaimTypes.Name)?.Value ?? "Unknown";
        return Task.FromResult($"User {username} is banned");
    }

    [Authorize(Roles = TestRoles.ADMIN)]
    public Task<string> GetAdminInfo()
    {
        CheckAuthorization(TestRoles.ADMIN); // Manual check for admin role
        var user = this.GetCurrentUser();
        var username = user.FindFirst(System.Security.Claims.ClaimTypes.Name)?.Value ?? "Unknown";
        return Task.FromResult($"Admin info for {username}: You have admin privileges");
    }

    [AllowAnonymous]
    public Task<string> GetPublicInfo()
    {
        // No authorization check for anonymous methods
        return Task.FromResult("This is public information - no authorization required");
    }

    [Authorize(Roles = TestRoles.MODERATOR)]
    public Task<string> ModifyUser()
    {
        CheckAuthorization(TestRoles.MODERATOR); // Manual check for moderator role
        var user = this.GetCurrentUser();
        var username = user.FindFirst(System.Security.Claims.ClaimTypes.Name)?.Value ?? "Unknown";
        return Task.FromResult($"User {username} has been modified");
    }

    public Task<string> AddToList()
    {
        CheckAuthorization(); // Manual check - requires authentication (class has [Authorize])
        var user = this.GetCurrentUser();
        var username = user.FindFirst(System.Security.Claims.ClaimTypes.Name)?.Value ?? "Unknown";
        return Task.FromResult($"User {username} added to list");
    }
}