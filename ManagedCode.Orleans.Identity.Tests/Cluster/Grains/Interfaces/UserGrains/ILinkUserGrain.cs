using ManagedCode.Orleans.Identity.Core.Interfaces.UserGrains;

namespace ManagedCode.Orleans.Identity.Tests.Cluster.Grains.Interfaces.UserGrains
{
    public interface ILinkUserGrain : IBaseTestUserGrain, IMagicLinkTokenUserGrain
    {
    }
}
