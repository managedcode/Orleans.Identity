using ManagedCode.Communication;
using ManagedCode.Orleans.Identity.Tests.Cluster.Grains.Interfaces.UserGrains;

namespace ManagedCode.Orleans.Identity.Tests.Cluster.Grains.UserGrains;

public class BaseTestUserGrain : Grain, IBaseTestUserGrain
{
    protected bool _tokenInvalid;
    protected bool _tokenExpired;
    protected bool _tokenValid;
    protected string _tokenValue;
    
    public ValueTask<Result<bool>> IsTokenExpired()
    {
        return Result<bool>.Succeed(_tokenExpired).AsValueTask();
    }

    public ValueTask<Result<bool>> IsTokenValid()
    {
        return Result<bool>.Succeed(_tokenValid).AsValueTask();
    }

    public ValueTask<Result<bool>> IsTokenInvalid()
    {
        return Result<bool>.Succeed(_tokenInvalid).AsValueTask();
    }

    public ValueTask<Result<string>> GetTokenValue()
    {
        return Result<string>.Succeed(_tokenValue).AsValueTask();
    }
}