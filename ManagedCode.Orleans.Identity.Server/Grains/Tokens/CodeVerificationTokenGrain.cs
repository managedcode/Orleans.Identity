using ManagedCode.Communication;
using ManagedCode.Orleans.Identity.Constants;
using ManagedCode.Orleans.Identity.Interfaces.TokenGrains;
using ManagedCode.Orleans.Identity.Models;
using Orleans.Runtime;
using System.Threading.Tasks;
using ManagedCode.Orleans.Identity.Server.Constants;
using ManagedCode.Orleans.Identity.Server.Grains.Tokens.Base;

namespace ManagedCode.Orleans.Identity.Server.Grains.Tokens
{
    public class CodeVerificationTokenGrain : TokenGrain, ITokenCodeVerificaitonGrain
    {
        public CodeVerificationTokenGrain(
        [PersistentState("verificationCodeToken", OrleansIdentityConstants.TOKEN_STORAGE_NAME)]
        IPersistentState<TokenModel> tokenState) : base(tokenState, TokenGrainConstants.EMAIL_VERIFICATION_TOKEN_REMINDER_NAME)
        {
        }
    }
}
