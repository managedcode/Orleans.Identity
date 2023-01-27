
using ManagedCode.Orleans.Identity.Constants;
using ManagedCode.Orleans.Identity.Interfaces.TokenGrains;
using ManagedCode.Orleans.Identity.Models;
using ManagedCode.Orleans.Identity.Server.Constants;
using ManagedCode.Orleans.Identity.Server.Grains.Tokens.Base;
using Orleans;
using Orleans.Runtime;

namespace ManagedCode.Orleans.Identity.Server.Grains.Tokens;

public class EmailVerificationTokenGrain : TokenGrain, IEmailVerificationTokenGrain
{
    public EmailVerificationTokenGrain(
        [PersistentState("emailVerificationToken", OrleansIdentityConstants.TOKEN_STORAGE_NAME)]
        IPersistentState<TokenModel> tokenState) : base(tokenState, TokenGrainConstants.EMAIL_VERIFICATION_TOKEN_REMINDER_NAME)
    {
    }
}