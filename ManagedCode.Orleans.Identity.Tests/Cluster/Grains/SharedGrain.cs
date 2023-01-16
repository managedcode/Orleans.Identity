using ManagedCode.Orleans.Identity.Tests.Cluster.Grains.Interfaces;

namespace ManagedCode.Orleans.Identity.Tests.Cluster.Grains
{
    public class SharedGrain : Grain, ISharedGrain
    {
        public Task<string> GetInfo()
        {
            return Task.FromResult("info");
        }
    }
}
