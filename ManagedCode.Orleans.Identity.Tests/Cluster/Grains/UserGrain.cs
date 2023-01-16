using ManagedCode.Orleans.Identity.Tests.Cluster.Grains.Interfaces;
using Microsoft.AspNetCore.Authorization;

namespace ManagedCode.Orleans.Identity.Tests.Cluster.Grains;

public class UserGrain : Grain, IUserGrain
{

    [Authorize]
    public Task<string> GetUser()
    {
        return Task.FromResult("user");
    }
}