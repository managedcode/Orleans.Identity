using System.Threading.Tasks;
using ManagedCode.Communication;
using Orleans;

namespace ManagedCode.Orleans.Identity.Core.Interfaces.UserGrains;

public interface IMagicLinkTokenUserGrain : IGrainWithStringKey
{
    ValueTask<Result> MagicLinkTokenExpiredAsync(string token);
    
    ValueTask<Result> MagicLinkTokenValidAsync(string token);
}