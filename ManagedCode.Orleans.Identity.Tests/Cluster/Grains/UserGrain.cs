using ManagedCode.Orleans.Identity.Tests.Cluster.Grains.Interfaces;
using ManagedCode.Orleans.Identity.Tests.Constants;
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
}