using ManagedCode.Communication;
using System.Threading.Tasks;
using Orleans;

namespace ManagedCode.Orleans.Identity.Interfaces.UserGrain
{
    public interface IEmailVerificationTokenUserGrain : IGrainWithStringKey
    {
        ValueTask<Result> EmailVerificationTokenExpiredAsync(string token);

        ValueTask<Result> EmailVerificationTokenValidAsync(string token);
        
        ValueTask<Result> EmailVerificationTokenInvalidAsync(string token);
    }
}
