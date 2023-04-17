using System.Threading.Tasks;
using ManagedCode.Communication;
using ManagedCode.Orleans.Identity.Core.Models;
using Orleans;

namespace ManagedCode.Orleans.Identity.Core.Interfaces.TokenGrains;

public interface IBaseTokenGrain : IGrainWithStringKey
{
    ValueTask<Result> CreateAsync(CreateTokenModel createModel);

    ValueTask<Result> VerifyAsync();

    ValueTask<Result<TokenModel>> GetTokenAsync();
}