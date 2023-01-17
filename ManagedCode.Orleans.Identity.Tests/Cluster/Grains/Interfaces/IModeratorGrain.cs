namespace ManagedCode.Orleans.Identity.Tests.Cluster.Grains.Interfaces
{
    public interface IModeratorGrain : IGrainWithStringKey
    {
        Task<string> GetInfo();
        Task<string> GetModerators();
        Task<string> GetPublicInformation();
    }
}
