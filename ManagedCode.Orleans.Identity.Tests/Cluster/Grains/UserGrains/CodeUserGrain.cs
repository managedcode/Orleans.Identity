using ManagedCode.Communication;
using ManagedCode.Orleans.Identity.Tests.Cluster.Grains.Interfaces.UserGrains;

namespace ManagedCode.Orleans.Identity.Tests.Cluster.Grains.UserGrains;

public class CodeUserGrain : BaseTestUserGrain, ICodeUserGrain
{
    public ValueTask<Result> CodeVerificationTokenExpiredAsync(string token)
    {
        _tokenValue = token;
        _tokenExpired = true;
        return Result.Succeed().AsValueTask();
    }

    public ValueTask<Result> CodeVerificationTokenInvalidAsync(string token)
    {
        _tokenValue = token;
        _tokenInvalid = true;
        return Result.Succeed().AsValueTask();
    }

    public ValueTask<Result> CodeVerificationTokenValidAsync(string token)
    {
        _tokenValue = token;
        _tokenValid = true;
        return Result.Succeed().AsValueTask();
    }
}
