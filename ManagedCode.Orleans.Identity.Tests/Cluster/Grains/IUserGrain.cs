namespace ManagedCode.Orleans.Identity.Tests.Cluster.Grains;

public interface IUserGrain : IGrainWithStringKey
{
    Task<string> GetUser();
    Task<string> BanUser();
    Task<string> GetAdminInfo();
    Task<string> GetPublicInfo();
    Task<string> ModifyUser();
    Task<string> AddToList();
}