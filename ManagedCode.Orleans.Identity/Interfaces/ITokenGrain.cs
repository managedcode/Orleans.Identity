using System.Threading.Tasks;
using ManagedCode.Communication;
using ManagedCode.Orleans.Identity.Models;
using Orleans;

namespace ManagedCode.Orleans.Identity.Interfaces;

public interface ITokenGrain : IGrainWithStringKey
{
    ValueTask<Result> CreateAsync(TokenModel tokenModel);

    ValueTask<Result<TokenModel>> GetTokenAsync();

    ValueTask<Result<bool>> CompareTokensAsync(string token);

    ValueTask<Result> ClearTokenAsync();

    ValueTask<Result<bool>> IsExpiredAsync();
}