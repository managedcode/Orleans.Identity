using System;
using System.Threading.Tasks;
using ManagedCode.Communication;
using ManagedCode.Orleans.Identity.Constants;
using ManagedCode.Orleans.Identity.Extensions;
using ManagedCode.Orleans.Identity.Interfaces.TokenGrains;
using ManagedCode.Orleans.Identity.Models;
using ManagedCode.Orleans.Identity.Server.Constants;
using Orleans;
using Orleans.Runtime;

namespace ManagedCode.Orleans.Identity.Server.Grains.Tokens;

public class EmailVerificationTokenGrain : Grain, IEmailVerificationTokenGrain, IRemindable
{
    private readonly IPersistentState<TokenModel> _tokenState;

    public EmailVerificationTokenGrain(
        [PersistentState("emailVerificationToken", OrleansIdentityConstants.TOKEN_STORAGE_NAME)]
        IPersistentState<TokenModel> tokenState)
    {
        _tokenState = tokenState;
    }
    
    private async Task OnTimerTicked(object args)
    {
        if (_tokenState.RecordExists is false)
            return;

        _tokenState.State.IsActive = false;

        await _tokenState.WriteStateAsync();
    }
    
    public async ValueTask<Result> CreateAsync(CreateTokenModel createModel)
    {
        // TODO: add to the method description that if token lifetime less than 1 minute, timer with period 1 minute will be registered 
        if (createModel.IsModelValid() is false)
        {
            return Result.Fail();
        }

        _tokenState.State = new TokenModel
        {
            IsActive = true,
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
            await this.RegisterOrUpdateReminder(TokenGrainConstants.EMAIL_VERIFICATION_TOKEN_REMINDER_NAME, _tokenState.State.Lifetime, _tokenState.State.Lifetime);
        }

        return Result.Succeed();
    }

    public ValueTask<Result> VerifyAsync()
    {
        throw new System.NotImplementedException();
    }

    public async Task ReceiveReminder(string reminderName, TickStatus status)
    {
        if (_tokenState.RecordExists is false)
            return;

        if (reminderName == TokenGrainConstants.EMAIL_VERIFICATION_TOKEN_REMINDER_NAME)
        {
            _tokenState.State.IsActive = false;
            await _tokenState.WriteStateAsync();
        }
    }
}