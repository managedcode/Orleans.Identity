namespace ManagedCode.Orleans.Identity.Tests.Cluster.Grains.Interfaces
{
    public interface ISharedGrain : IGrainWithStringKey
    {
        Task<string> GetInfo();
    }
}
