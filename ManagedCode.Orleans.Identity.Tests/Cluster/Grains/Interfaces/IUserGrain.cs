namespace ManagedCode.Orleans.Identity.Tests.Cluster.Grains.Interfaces;

public interface IUserGrain : IGrainWithStringKey
{
    Task<string> GetUser();
    Task<string> BanUser();
    Task<string> GetPublicInfo();
}