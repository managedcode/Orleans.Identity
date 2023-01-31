using System.Threading.Tasks;
using ManagedCode.Communication;
using Orleans;

namespace ManagedCode.Orleans.Identity.Interfaces.UserGrain;

public interface IMagicLinkTokenUserGrain : IGrainWithStringKey
{
    ValueTask<Result> MagicLinkTokenExpiredAsync(string token);
    ValueTask<Result> MagicLinkTokenValidAsync(string token);
    ValueTask<Result> MagicLinkTokenInvalidAsync(string token);
}