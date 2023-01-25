using System.Threading.Tasks;
using ManagedCode.Communication;
using ManagedCode.Orleans.Identity.Constants;
using ManagedCode.Orleans.Identity.Interfaces.TokenGrains;
using ManagedCode.Orleans.Identity.Models;
using Orleans;
using Orleans.Runtime;

namespace ManagedCode.Orleans.Identity.Server.Grains.Tokens;

public class EmailVerificationTokenGrainGrain : Grain, IEmailVerificationTokenGrain
{
    private readonly IPersistentState<TokenModel> _tokenState;

    public EmailVerificationTokenGrainGrain(
        [PersistentState("emailVerificationToken", OrleansIdentityConstants.TOKEN_STORAGE_NAME)]
        IPersistentState<TokenModel> tokenState)
    {
        _tokenState = tokenState;
    }
    
    public ValueTask<Result> CreateAsync(CreateTokenModel createModel)
    {
        throw new System.NotImplementedException();
    }

    public ValueTask<Result> VerifyAsync(string token)
    {
        throw new System.NotImplementedException();
    }
}