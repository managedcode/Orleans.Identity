using ManagedCode.Communication;
using ManagedCode.Orleans.Identity.Tests.Cluster.Grains.Interfaces.UserGrains;

namespace ManagedCode.Orleans.Identity.Tests.Cluster.Grains.UserGrains
{
    public class LinkUserGrain : BaseTestUserGrain, ILinkUserGrain
    {

        public ValueTask<Result> MagicLinkTokenExpiredAsync(string token)
        {
            _tokenValue = token;
            _tokenExpired = true;
            return Result.Succeed().AsValueTask();
        }

        public ValueTask<Result> MagicLinkTokenInvalidAsync(string token)
        {
            _tokenValue = token;
            _tokenInvalid = true;
            return Result.Succeed().AsValueTask();
        }

        public ValueTask<Result> MagicLinkTokenValidAsync(string token)
        {
            _tokenValue = token;
            _tokenValid = true;
            return Result.Succeed().AsValueTask();
        }
    }   
}
