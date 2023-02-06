using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ManagedCode.Communication;
using ManagedCode.Orleans.Identity.Constants;
using ManagedCode.Orleans.Identity.Enums;
using ManagedCode.Orleans.Identity.Interfaces;
using ManagedCode.Orleans.Identity.Models;
using ManagedCode.Orleans.Identity.Options;
using ManagedCode.Orleans.Identity.Server.Constants;
using Orleans;
using Orleans.Runtime;

namespace ManagedCode.Orleans.Identity.Server.Grains;

public class SessionGrain : Grain, ISessionGrain, IRemindable
{
    private readonly SessionOption _sessionOption;
    private readonly IPersistentState<SessionModel> _sessionState;

    public SessionGrain(
        [PersistentState("sessions", OrleansIdentityConstants.SESSION_STORAGE_NAME)]
        IPersistentState<SessionModel> sessionState,
        SessionOption sessionOption)
    {
        _sessionState = sessionState;
        _sessionOption = sessionOption;
    }

    public ValueTask<Result<SessionModel>> GetSessionAsync()
    {
        if (_sessionState.RecordExists is false)
        {
            return Result<SessionModel>.Fail().AsValueTask();
        }

        var result = GetSessionModel();

        return Result<SessionModel>.Succeed(result).AsValueTask();
    }

    public async Task<Result<SessionModel>> CreateAsync(CreateSessionModel model)
    {
        var date = DateTime.UtcNow;

        _sessionState.State = new SessionModel(this.GetPrimaryKeyString())
        {
            IsActive = true,
            UserGrainId = model.UserGrainId,
            UserData = model.UserData ?? new Dictionary<string, HashSet<string>>(),
            Status = SessionStatus.Active,
            CreatedDate = date,
            LastAccess = date
        };

        await _sessionState.WriteStateAsync();

        var result = GetSessionModel();
        
        await SetSessionLifetimeReminder();
        
        return Result<SessionModel>.Succeed(result);
    }

    public async Task ReceiveReminder(string reminderName, TickStatus status)
    {
        if (reminderName == SessionGrainConstants.SESSION_LIFETIME_REMINDER_NAME)
        {
            await CloseAsync();
        }
    }

    public ValueTask<Result<ImmutableDictionary<string, HashSet<string>>>> ValidateAndGetClaimsAsync()
    {
        if (_sessionState.RecordExists is false)
        {
            DeactivateOnIdle();
            return Result<ImmutableDictionary<string, HashSet<string>>>.Fail().AsValueTask();
        }

        if (_sessionState.State.IsActive is false)
        {
            DeactivateOnIdle();
            return Result<ImmutableDictionary<string, HashSet<string>>>.Fail().AsValueTask();
        }

        _sessionState.State.LastAccess = DateTime.UtcNow;

        return Result<ImmutableDictionary<string, HashSet<string>>>.Succeed(_sessionState.State.UserData.ToImmutableDictionary()).AsValueTask();
    }

    public async Task<Result> CloseAsync()
    {
        if (_sessionState.RecordExists is false)
        {
            return Result.Fail();
        }

        if (_sessionOption.ClearStateOnClose)
        {
            await _sessionState.ClearStateAsync();
            return Result.Succeed();
        }

        _sessionState.State.ClosedDate = DateTime.UtcNow;
        _sessionState.State.Status = SessionStatus.Closed;
        _sessionState.State.IsActive = false;

        await _sessionState.WriteStateAsync();
        await UnregisterReminder();

        return Result.Succeed();
    }

    public ValueTask<Result> PauseSessionAsync()
    {
        if (_sessionState.RecordExists is false)
        {
            DeactivateOnIdle();
            return Result.Fail().AsValueTask();
        }

        _sessionState.State.LastAccess = DateTime.UtcNow;
        _sessionState.State.Status = SessionStatus.Paused;

        return Result.Succeed().AsValueTask();
    }

    public ValueTask<Result> ResumeSessionAsync()
    {
        if (_sessionState.RecordExists is false)
        {
            DeactivateOnIdle();
            return Result.Fail().AsValueTask();
        }

        _sessionState.State.LastAccess = DateTime.UtcNow;
        _sessionState.State.Status = SessionStatus.Active;

        return Result.Succeed().AsValueTask();
    }

    public ValueTask<Result> AddProperty(string key, string value)
    {
        if (_sessionState.RecordExists is false)
        {
            DeactivateOnIdle();
            return Result.Fail().AsValueTask();
        }

        if (_sessionState.State.UserData.ContainsKey(key))
        {
            return Result.Fail().AsValueTask();
        }

        _sessionState.State.UserData[key] = new HashSet<string> { value };
        return Result.Succeed().AsValueTask();
    }

    public ValueTask<Result> AddProperty(string key, List<string> values)
    {
        if (_sessionState.RecordExists is false)
        {
            DeactivateOnIdle();
            return Result.Fail().AsValueTask();
        }

        if (_sessionState.State.UserData.ContainsKey(key))
        {
            return Result.Fail().AsValueTask();
        }

        _sessionState.State.UserData[key] = values.ToHashSet();
        return Result.Succeed().AsValueTask();
    }

    public ValueTask<Result> ReplaceProperty(string key, string value)
    {
        if (_sessionState.RecordExists is false)
        {
            DeactivateOnIdle();
            return Result.Fail().AsValueTask();
        }

        if (_sessionState.State.UserData.ContainsKey(key) is false)
        {
            return Result.Fail().AsValueTask();
        }

        _sessionState.State.UserData[key] = new HashSet<string> { value };
        return Result.Succeed().AsValueTask();
    }

    public ValueTask<Result> ReplaceProperty(string key, List<string> values)
    {
        if (_sessionState.RecordExists is false)
        {
            DeactivateOnIdle();
            return Result.Fail().AsValueTask();
        }

        if (_sessionState.State.UserData.ContainsKey(key) is false)
        {
            return Result.Fail().AsValueTask();
        }

        _sessionState.State.UserData[key] = values.ToHashSet();
        return Result.Succeed().AsValueTask();
    }

    public ValueTask<Result> AddValueToProperty(string key, string value)
    {
        if (_sessionState.RecordExists is false)
        {
            DeactivateOnIdle();
            return Result.Fail().AsValueTask();
        }

        if (_sessionState.State.UserData.TryGetValue(key, out var hashset))
        {
            return hashset.Add(value) ? Result.Succeed().AsValueTask() : Result.Fail().AsValueTask();
        }

        return Result.Fail().AsValueTask();
    }

    public ValueTask<Result> RemoveProperty(string key)
    {
        if (_sessionState.RecordExists is false)
        {
            DeactivateOnIdle();
            return Result.Fail().AsValueTask();
        }

        if (_sessionState.State.UserData.ContainsKey(key))
        {
            _sessionState.State.UserData.Remove(key);
            return Result.Succeed().AsValueTask();
        }

        return Result.Fail().AsValueTask();
    }

    public ValueTask<Result> RemoveValueFromProperty(string key, string value)
    {
        if (_sessionState.RecordExists is false)
        {
            DeactivateOnIdle();
            return Result.Fail().AsValueTask();
        }

        if (_sessionState.State.UserData.TryGetValue(key, out var hashset))
        {
            return hashset.Remove(value) ? Result.Succeed().AsValueTask() : Result.Fail().AsValueTask();
        }

        return Result.Fail().AsValueTask();
    }

    public ValueTask<Result> ClearUserData()
    {
        if (_sessionState.RecordExists is false)
        {
            DeactivateOnIdle();
            return Result.Fail().AsValueTask();
        }

        _sessionState.State.UserData.Clear();
        return Result.Succeed().AsValueTask();
    }

    public override async Task OnDeactivateAsync(DeactivationReason reason, CancellationToken cancellationToken)
    {
        if (_sessionState.RecordExists)
        {
            await _sessionState.WriteStateAsync();
        }
        else
        {
            await _sessionState.ClearStateAsync();
        }
    }

    private SessionModel GetSessionModel()
    {
        return new SessionModel(_sessionState.State.Id)
        {
            IsActive = _sessionState.State.IsActive,
            ClosedDate = _sessionState.State.ClosedDate,
            CreatedDate = _sessionState.State.CreatedDate,
            LastAccess = _sessionState.State.LastAccess,
            Status = _sessionState.State.Status,
            UserData = _sessionState.State.UserData
        };
    }

    private async ValueTask SetSessionLifetimeReminder()
    {
        if (_sessionOption.SessionLifetime == TimeSpan.Zero ||
            _sessionOption.SessionLifetime < TimeSpan.FromMinutes(1))
        {
            return;   
        }

        var reminder = await this.GetReminder(SessionGrainConstants.SESSION_LIFETIME_REMINDER_NAME);
        if(reminder is not null)
            return;
        
        // TODO: Unregister reminder when session is closed or deleted 
        await this.RegisterOrUpdateReminder(SessionGrainConstants.SESSION_LIFETIME_REMINDER_NAME,
            _sessionOption.SessionLifetime, _sessionOption.SessionLifetime);

    }

    private async ValueTask UnregisterReminder()
    {
        var reminder = await this.GetReminder(SessionGrainConstants.SESSION_LIFETIME_REMINDER_NAME);
        if (reminder is not null)
            await this.UnregisterReminder(reminder);
    }
}