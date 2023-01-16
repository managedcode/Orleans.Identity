namespace ManagedCode.Orleans.Identity.Tests.Cluster.Grains.Interfaces;

public interface IPublicGrain : IGrainWithStringKey
{
    Task<string> CommonMethod();
    Task<string> AuthorizedMethod();
    Task<string> AdminOnly();
    Task<string> ModeratorOnly();
}