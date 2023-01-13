using ManagedCode.Orleans.Identity.Tests.Cluster.Grains.Interfaces;
using Microsoft.AspNetCore.Authorization;

namespace ManagedCode.Orleans.Identity.Tests.Cluster.Grains;

[Authorize]
public class UserGrain : Grain, IUserGrain
{
    public Task<string> GetUser()
    {
        return Task.FromResult("user");
    }
}