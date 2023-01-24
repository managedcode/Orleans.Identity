using System.Threading.Tasks;
using ManagedCode.Communication;
using ManagedCode.Orleans.Identity.Constants;
using ManagedCode.Orleans.Identity.Interfaces;
using ManagedCode.Orleans.Identity.Models;
using Orleans;
using Orleans.Runtime;

namespace ManagedCode.Orleans.Identity.Server.Grains;

public class TokenGrain : Grain, ITokenGrain
{
    private readonly IPersistentState<TokenModel> _tokenState;

    public TokenGrain(
        [PersistentState("tokens", OrleansIdentityConstants.TOKEN_STORAGE_NAME)]
        IPersistentState<TokenModel> tokenState)
    {
        _tokenState = tokenState;
    }

    public async ValueTask<Result> AddToken(TokenModel tokenModel)
    {
        _tokenState.State = tokenModel;

        await _tokenState.WriteStateAsync();
        
        return Result.Succeed();
    }

    public ValueTask<Result<TokenModel>> GetToken()
    {
        if (_tokenState.RecordExists is false)
        {
            DeactivateOnIdle();
            return Result.Fail<TokenModel>().AsValueTask();
        }

        return Result<TokenModel>.Succeed(_tokenState.State).AsValueTask();
    }

    public async ValueTask<Result> ClearToken()
    {
        if (_tokenState.RecordExists is false)
        {
            DeactivateOnIdle();
            return Result.Fail();
        }

        await _tokenState.ClearStateAsync();
        return Result.Succeed();
    }
}