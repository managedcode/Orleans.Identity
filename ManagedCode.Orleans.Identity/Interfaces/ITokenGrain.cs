using System.Threading.Tasks;
using ManagedCode.Communication;
using ManagedCode.Orleans.Identity.Models;
using Orleans;

namespace ManagedCode.Orleans.Identity.Interfaces;

public interface ITokenGrain : IGrainWithStringKey
{
    ValueTask<Result> AddToken(TokenModel tokenModel);

    ValueTask<Result<TokenModel>> GetToken();

    ValueTask<Result<bool>> CompareTokens(string token);

    ValueTask<Result> ClearToken();

    ValueTask<Result<bool>> IsExpired();
}