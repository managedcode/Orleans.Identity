using System.Threading.Tasks;
using ManagedCode.Communication;
using Orleans;

namespace ManagedCode.Orleans.Identity.Interfaces.UserGrains;

public interface ICodeVerificationTokenUserGrain : IGrainWithStringKey
{
    ValueTask<Result> CodeVerificationTokenExpiredAsync(string token);
        
    ValueTask<Result> CodeVerificationTokenValidAsync(string token);
        
    ValueTask<Result> CodeVerificationTokenInvalidAsync(string token);    
}