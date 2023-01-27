using ManagedCode.Communication;
using ManagedCode.Orleans.Identity.Extensions;
using ManagedCode.Orleans.Identity.Interfaces.TokenGrains;
using ManagedCode.Orleans.Identity.Models;
using ManagedCode.Orleans.Identity.Server.Constants;
using Orleans;
using Orleans.Runtime;
using System;
using System.Threading.Tasks;

namespace ManagedCode.Orleans.Identity.Server.Grains.Tokens.Base
{
    public abstract class TokenGrain : Grain, IBaseTokenGrain
    {
        private readonly IPersistentState<TokenModel> _tokenState;


        protected async Task OnTimerTicked(object args)
        {
            if (_tokenState.RecordExists is false)
            {
                DeactivateOnIdle();
                return;
            }

            // TODO: also notify UserGrain if token is expired
            DeactivateOnIdle();
            await _tokenState.ClearStateAsync();
        }

        public async ValueTask<Result> CreateAsync(CreateTokenModel createModel)
        {
            if (createModel.IsModelValid() is false)
            {
                DeactivateOnIdle();
                return Result.Fail();
            }

            _tokenState.State = new TokenModel
            {
                Lifetime = createModel.Lifetime,
                UserGrainId = createModel.UserGrainId,
                Value = createModel.Value,
            };

            await _tokenState.WriteStateAsync();

            if (createModel.Lifetime < TimeSpan.FromMinutes(1))
            {
                RegisterTimer(OnTimerTicked, null, TimeSpan.FromMinutes(1), TimeSpan.FromMinutes(1));
            }
            else
            {
                await this.RegisterOrUpdateReminder(TokenGrainConstants.MAGIC_LINK_TOKEN_REMINDER_NAME, _tokenState.State.Lifetime, _tokenState.State.Lifetime);
            }

            return Result.Succeed();
        }


        public ValueTask<Result> VerifyAsync()
        {
            throw new System.NotImplementedException();
        }
    }
}
