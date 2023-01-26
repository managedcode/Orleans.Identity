using System;
using System.Threading.Tasks;
using ManagedCode.Communication;
using ManagedCode.Orleans.Identity.Constants;
using ManagedCode.Orleans.Identity.Extensions;
using ManagedCode.Orleans.Identity.Interfaces.TokenGrains;
using ManagedCode.Orleans.Identity.Models;
using Orleans;
using Orleans.Runtime;

namespace ManagedCode.Orleans.Identity.Server.Grains.Tokens;

public class EmailVerificationTokenGrain : Grain, IEmailVerificationTokenGrain
{
    private readonly IPersistentState<TokenModel> _tokenState;

    public EmailVerificationTokenGrain(
        [PersistentState("emailVerificationToken", OrleansIdentityConstants.TOKEN_STORAGE_NAME)]
        IPersistentState<TokenModel> tokenState)
    {
        _tokenState = tokenState;
    }
    
    
    
    public async ValueTask<Result> CreateAsync(CreateTokenModel createModel)
    {
        // TODO: add to the method description that if token lifetime less than 1 minute 
        if (createModel.IsModelValid() is false)
        {
            return Result.Fail();
        }

        if (createModel.Lifetime < TimeSpan.FromMinutes(1))
        {
                  
        }
        
        _tokenState.State = new TokenModel
        {
            IsActive = true,
            Lifetime = createModel.Lifetime,
            UserGrainId = createModel.UserGrainId,
            Value = createModel.Value,
        };

        await _tokenState.WriteStateAsync();

        return Result.Succeed();
    }

    public ValueTask<Result> VerifyAsync()
    {
        throw new System.NotImplementedException();
    }
}