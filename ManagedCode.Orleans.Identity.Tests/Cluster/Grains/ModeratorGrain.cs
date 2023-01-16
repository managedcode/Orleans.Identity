using ManagedCode.Orleans.Identity.Tests.Cluster.Grains.Interfaces;
using ManagedCode.Orleans.Identity.Tests.Constants;
using Microsoft.AspNetCore.Authorization;

namespace ManagedCode.Orleans.Identity.Tests.Cluster.Grains
{
    [Authorize(Roles = TestRoles.MODERATOR)]
    public class ModeratorGrain : Grain, IModeratorGrain
    {
        public Task<string> GetInfo()
        {
            return Task.FromResult("info");
        }
    }
}
