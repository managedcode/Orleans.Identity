using ManagedCode.Communication;
using ManagedCode.Orleans.Identity.Constants;
using ManagedCode.Orleans.Identity.Interfaces.TokenGrains;
using ManagedCode.Orleans.Identity.Interfaces.UserGrains;
using ManagedCode.Orleans.Identity.Models;
using ManagedCode.Orleans.Identity.Server.Constants;
using ManagedCode.Orleans.Identity.Server.Grains.Tokens.Base;
using Orleans.Runtime;
using System.Threading.Tasks;

namespace ManagedCode.Orleans.Identity.Server.Grains.Tokens
{
    public class CodeVerificationTokenGrain : TokenGrain, ICodeVerificationTokenGrain
    {
        public CodeVerificationTokenGrain(
        [PersistentState("verificationCodeToken", OrleansIdentityConstants.SESSION_STORAGE)]
        IPersistentState<TokenModel> tokenState) : base(tokenState, TokenGrainConstants.EMAIL_VERIFICATION_TOKEN_REMINDER_NAME)
        {
        }

        protected override async ValueTask CallUserGrainOnTokenExpired()
        {
            if (TokenState.State.UserGrainId.IsDefault)
            {
                return;
            }
            var parseResult = TokenState.State.UserGrainId.Key.ToString();
            var userGrain = GrainFactory.GetGrain<ICodeVerificationTokenUserGrain>(parseResult);
            await userGrain.CodeVerificationTokenExpiredAsync(TokenState.State.Value);
        }

        protected override async ValueTask CallUserGrainOnTokenValid()
        {
            if (TokenState.State.UserGrainId.IsDefault)
            {
                return;
            }
            var parseResult = TokenState.State.UserGrainId.Key.ToString();
            var userGrain = GrainFactory.GetGrain<ICodeVerificationTokenUserGrain>(parseResult);
            await userGrain.CodeVerificationTokenValidAsync(TokenState.State.Value);
        }
    }
}
