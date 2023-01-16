using ManagedCode.Orleans.Identity.Server.Tests.Cluster.Grains.Interfaces;
using ManagedCode.Orleans.Identity.Server.Tests.Constants;
using Microsoft.AspNetCore.Authorization;

namespace ManagedCode.Orleans.Identity.Tests.Cluster.Grains;

[Authorize]
public class UserGrain : Grain, IUserGrain
{
    [Authorize]
    public Task<string> GetUser()
    {
        return Task.FromResult("user");
    }

    [Authorize(Roles = TestRoles.ADMIN)]
    public Task<string> BanUser()
    {
        return Task.FromResult("User is banned");
    }

    [AllowAnonymous]
    public Task<string> GetPublicInfo()
    {
        return Task.FromResult("public info");
    }

    [Authorize(Roles = TestRoles.MODERATOR)]
    public Task<string> ModifyUser()
    {
        return Task.FromResult("user modified");
    }

    public Task<string> AddToList()
    {
        return Task.FromResult("add to list");
    }
}