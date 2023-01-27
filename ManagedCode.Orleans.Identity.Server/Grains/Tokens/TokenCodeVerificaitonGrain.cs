using ManagedCode.Communication;
using ManagedCode.Orleans.Identity.Constants;
using ManagedCode.Orleans.Identity.Interfaces.TokenGrains;
using ManagedCode.Orleans.Identity.Models;
using Orleans.Runtime;
using System.Threading.Tasks;

namespace ManagedCode.Orleans.Identity.Server.Grains.Tokens
{
    public class TokenCodeVerificaitonGrain : ITokenCodeVerificaitonGrain
    {
        private readonly IPersistentState<TokenModel> _tokenState;

        public TokenCodeVerificaitonGrain(
        [PersistentState("magicLinkToken", OrleansIdentityConstants.TOKEN_STORAGE_NAME)]
        IPersistentState<TokenModel> tokenState)
        {
            _tokenState = tokenState;
        }

        public ValueTask<Result> CreateAsync(CreateTokenModel createModel)
        {
            throw new System.NotImplementedException();
        }

        public ValueTask<Result> VerifyAsync()
        {
            throw new System.NotImplementedException();
        }
    }
}
