using ManagedCode.Communication;
using System.Threading.Tasks;

namespace ManagedCode.Orleans.Identity.Interfaces.UserGrain
{
    public interface IEmailVerificationUserGrain
    {
        ValueTask<Result> EmailVerificationTokenExpiredAsync();

        ValueTask<Result> EmailVerificationTokenValidAsync();
    }
}
