using ManagedCode.Communication;

namespace ManagedCode.Orleans.Identity.Tests.Cluster.Grains.Interfaces.UserGrains;

public interface IBaseTestUserGrain : IGrainWithStringKey
{
    ValueTask<Result<bool>> IsTokenExpired();

    ValueTask<Result<bool>> IsTokenValid();

    ValueTask<Result<bool>> IsTokenInvalid();

    ValueTask<Result<string>> GetTokenValue();
}