using ManagedCode.Communication;
using ManagedCode.Orleans.Identity.Extensions;
using ManagedCode.Orleans.Identity.Interfaces.TokenGrains;
using ManagedCode.Orleans.Identity.Models;
using ManagedCode.Orleans.Identity.Server.Constants;
using Orleans;
using Orleans.Runtime;
using System;
using System.Threading.Tasks;
using ManagedCode.Orleans.Identity.Constants;

namespace ManagedCode.Orleans.Identity.Server.Grains.Tokens.Base
{
    public abstract class TokenGrain : Grain, IBaseTokenGrain, IRemindable
    {
        private readonly string _reminderName;
        protected readonly IPersistentState<TokenModel> TokenState;

        private IDisposable? _timerReference;

        protected TokenGrain(IPersistentState<TokenModel> tokenState, string reminderName)
        {
            TokenState = tokenState;
            _reminderName = reminderName;
        }

        protected abstract ValueTask CallUserGrainOnTokenExpired();
        protected abstract ValueTask CallUserGrainOnTokenValid();
        
        private async Task OnTimerTicked(object args)
        {            
            _timerReference?.Dispose();
            if (TokenState.RecordExists is false)
            {
                DeactivateOnIdle();
                return;
            }
            
            await CallUserGrainOnTokenExpired();
            await TokenState.ClearStateAsync();
            DeactivateOnIdle();
        }

        public async ValueTask<Result> CreateAsync(CreateTokenModel createModel)
        {
            if (createModel.IsModelValid() is false)
            {
                DeactivateOnIdle();
                return Result.Fail();
            }

            TokenState.State = new TokenModel
            {
                Lifetime = createModel.Lifetime,
                UserGrainId = createModel.UserGrainId,
                Value = createModel.Value,
            };

            await TokenState.WriteStateAsync();

            if (createModel.Lifetime < TimeSpan.FromMinutes(1))
            {
                _timerReference = RegisterTimer(OnTimerTicked, null, createModel.Lifetime, createModel.Lifetime);
            }
            else
            {
                await this.RegisterOrUpdateReminder(_reminderName, TokenState.State.Lifetime, TokenState.State.Lifetime);
            }

            return Result.Succeed();
        }


        public async ValueTask<Result> VerifyAsync()
        {
            if (TokenState.RecordExists is false)
            {
                DeactivateOnIdle();
                return Result.Fail();    
            }

            await CallUserGrainOnTokenValid();
            return Result.Succeed();
        }

        public ValueTask<Result<TokenModel>> GetTokenAsync()
        {
            if (TokenState.RecordExists is false)
            {
                DeactivateOnIdle();
                return Result<TokenModel>.Fail().AsValueTask();
            }

            return Result<TokenModel>.Succeed(TokenState.State).AsValueTask();
        }

        public async Task ReceiveReminder(string reminderName, TickStatus status)
        {
            if (TokenState.RecordExists is false)
            {
                DeactivateOnIdle();
                await this.UnregisterReminder(await this.GetReminder(reminderName));
                return;
            }

            if (reminderName == _reminderName)
            {
                await CallUserGrainOnTokenExpired();
                await this.UnregisterReminder(await this.GetReminder(reminderName));
                await TokenState.ClearStateAsync();
                DeactivateOnIdle();
            }
        }
    }
}
