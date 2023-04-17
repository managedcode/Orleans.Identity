using ManagedCode.Communication;
using System.Threading.Tasks;
using Orleans;

namespace ManagedCode.Orleans.Identity.Core.Interfaces.UserGrains
{
    public interface IEmailVerificationTokenUserGrain : IGrainWithStringKey
    {
        ValueTask<Result> EmailVerificationTokenExpiredAsync(string token);

        ValueTask<Result> EmailVerificationTokenValidAsync(string token);
    }
}
