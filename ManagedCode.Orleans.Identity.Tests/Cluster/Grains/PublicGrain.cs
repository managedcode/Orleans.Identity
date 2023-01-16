using ManagedCode.Orleans.Identity.Server.Tests.Cluster.Grains.Interfaces;
using ManagedCode.Orleans.Identity.Server.Tests.Constants;
using Microsoft.AspNetCore.Authorization;

namespace ManagedCode.Orleans.Identity.Tests.Cluster.Grains;

public class PublicGrain : Grain, IPublicGrain
{
    public Task<string> CommonMethod()
    {
        return Task.FromResult("common");
    }

    [Authorize]
    public Task<string> AuthorizedMethod()
    {
        return Task.FromResult("authorized");
    }

    [Authorize(Roles = TestRoles.ADMIN)]
    public Task<string> AdminOnly()
    {
        return Task.FromResult("admin only");
    }

    [Authorize(Roles = TestRoles.MODERATOR)]
    public Task<string> ModeratorOnly()
    {
        return Task.FromResult("moderator only");
    }
}