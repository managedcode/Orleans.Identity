using ManagedCode.Communication;
using ManagedCode.Orleans.Identity.Constants;
using ManagedCode.Orleans.Identity.Extensions;
using ManagedCode.Orleans.Identity.Interfaces.TokenGrains;
using ManagedCode.Orleans.Identity.Models;
using ManagedCode.Orleans.Identity.Server.Constants;
using Orleans;
using Orleans.Runtime;
using System;
using System.Threading.Tasks;
using ManagedCode.Orleans.Identity.Server.Grains.Tokens.Base;

namespace ManagedCode.Orleans.Identity.Server.Grains.Tokens;

public class MagicLinkTokenGrain : TokenGrain, IMagicLinkTokenGrain
{
    public MagicLinkTokenGrain(
        [PersistentState("magicLinkToken", OrleansIdentityConstants.TOKEN_STORAGE_NAME)]
        IPersistentState<TokenModel> tokenState) : base(tokenState, TokenGrainConstants.MAGIC_LINK_TOKEN_REMINDER_NAME)
    {
    }
}