using ManagedCode.Communication;
using ManagedCode.Orleans.Identity.Tests.Cluster.Grains.Interfaces.UserGrains;

namespace ManagedCode.Orleans.Identity.Tests.Cluster.Grains.UserGrains;

public class SocialUserGrain : BaseTestUserGrain, ISocialUserGrain
{
    public ValueTask<Result> EmailVerificationTokenExpiredAsync(string token)
    {
        _tokenValue = token;
        _tokenExpired = true;
        return Result.Succeed().AsValueTask();
    }

    public ValueTask<Result> EmailVerificationTokenValidAsync(string token)
    {
        _tokenValue = token;
        _tokenValid = true;
        return Result.Succeed().AsValueTask();
    }

    public ValueTask<Result> EmailVerificationTokenInvalidAsync(string token)
    {
        _tokenValue = token;
        _tokenInvalid = true;
        return Result.Succeed().AsValueTask();
    }

}