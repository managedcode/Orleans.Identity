using ManagedCode.Orleans.Identity.Constants;
using ManagedCode.Orleans.Identity.Interfaces.TokenGrains;
using ManagedCode.Orleans.Identity.Interfaces.UserGrains;
using ManagedCode.Orleans.Identity.Models;
using ManagedCode.Orleans.Identity.Server.Constants;
using ManagedCode.Orleans.Identity.Server.Grains.Tokens.Base;
using Orleans.Runtime;
using System.Threading.Tasks;

namespace ManagedCode.Orleans.Identity.Server.Grains.Tokens;

public class MagicLinkTokenGrain : TokenGrain, IMagicLinkTokenGrain
{
    public MagicLinkTokenGrain(
        [PersistentState("magicLinkToken", OrleansIdentityConstants.TOKEN_STORAGE_NAME)]
        IPersistentState<TokenModel> tokenState) : base(tokenState, TokenGrainConstants.MAGIC_LINK_TOKEN_REMINDER_NAME)
    {
    }

    protected override async ValueTask CallUserGrainOnTokenExpired()
    {
        if (_tokenState.State.UserGrainId.IsDefault || _tokenState.State.UserGrainId.TryGetGuidKey(out var guid, out var grainId) is false)
        {
            return;
        }

        var userGrain = GrainFactory.GetGrain<IMagicLinkTokenUserGrain>(grainId);
        await userGrain.MagicLinkTokenExpiredAsync(_tokenState.State.Value);
    }

    protected override async ValueTask CallUserGrainOnTokenInvalid()
    {
        if (_tokenState.State.UserGrainId.IsDefault || _tokenState.State.UserGrainId.TryGetGuidKey(out var guid, out var grainId) is false)
        {
            return;
        }

        var userGrain = GrainFactory.GetGrain<IMagicLinkTokenUserGrain>(grainId);
        await userGrain.MagicLinkTokenInvalidAsync(_tokenState.State.Value);
    }

    protected override async ValueTask CallUserGrainOnTokenValid()
    {
        if (_tokenState.State.UserGrainId.IsDefault || _tokenState.State.UserGrainId.TryGetGuidKey(out var guid, out var grainId) is false)
        {
            return;
        }

        var userGrain = GrainFactory.GetGrain<IMagicLinkTokenUserGrain>(grainId);
        await userGrain.MagicLinkTokenValidAsync(_tokenState.State.Value);
    }
}