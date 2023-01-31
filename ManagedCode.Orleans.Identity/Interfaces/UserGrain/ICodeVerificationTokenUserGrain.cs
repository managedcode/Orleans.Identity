using System.Threading.Tasks;
using ManagedCode.Communication;

namespace ManagedCode.Orleans.Identity.Interfaces.UserGrain;

public interface ICodeVerificationTokenUserGrain
{
    ValueTask<Result> CodeVerificationTokenExpiredAsync(string token);
        
    ValueTask<Result> CodeVerificationTokenValidAsync(string token);
        
    ValueTask<Result> CodeVerificationTokenInvalidAsync(string token);    
}