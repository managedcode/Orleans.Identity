using ManagedCode.Communication;
using ManagedCode.Orleans.Identity.Constants;
using ManagedCode.Orleans.Identity.Interfaces.TokenGrains;
using ManagedCode.Orleans.Identity.Models;
using Orleans.Runtime;
using System.Threading.Tasks;
using ManagedCode.Orleans.Identity.Server.Constants;
using ManagedCode.Orleans.Identity.Server.Grains.Tokens.Base;
using ManagedCode.Orleans.Identity.Interfaces.UserGrains;

namespace ManagedCode.Orleans.Identity.Server.Grains.Tokens
{
    public class CodeVerificationTokenGrain : TokenGrain, ICodeVerificationTokenGrain
    {
        public CodeVerificationTokenGrain(
        [PersistentState("verificationCodeToken", OrleansIdentityConstants.TOKEN_STORAGE_NAME)]
        IPersistentState<TokenModel> tokenState) : base(tokenState, TokenGrainConstants.EMAIL_VERIFICATION_TOKEN_REMINDER_NAME)
        {
        }

        protected override async ValueTask CallUserGrainOnTokenExpired()
        {
            if(_tokenState.State.UserGrainId.IsDefault || _tokenState.State.UserGrainId.TryGetGuidKey(out var guid, out var grainId) is false)
            {
                return;
            }

            var userGrain = GrainFactory.GetGrain<ICodeVerificationTokenUserGrain>(grainId);
            await userGrain.CodeVerificationTokenExpiredAsync(_tokenState.State.Value);
        }

        protected override async ValueTask CallUserGrainOnTokenValid()
        {
            if (_tokenState.State.UserGrainId.IsDefault || _tokenState.State.UserGrainId.TryGetGuidKey(out var guid, out var grainId) is false)
            {
                return;
            }

            var userGrain = GrainFactory.GetGrain<ICodeVerificationTokenUserGrain>(grainId);
            await userGrain.CodeVerificationTokenValidAsync(_tokenState.State.Value);
        }
    }
}
