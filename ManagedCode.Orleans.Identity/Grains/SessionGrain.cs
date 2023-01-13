using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;
using ManagedCode.Communication;
using ManagedCode.Orleans.Identity.Grains.Interfaces;
using ManagedCode.Orleans.Identity.Models;
using ManagedCode.Orleans.Identity.Options;
using ManagedCode.Orleans.Identity.Shared.Constants;
using ManagedCode.Orleans.Identity.Shared.Enums;
using Orleans;
using Orleans.Runtime;

namespace ManagedCode.Orleans.Identity.Grains;

public class SessionGrain : Grain, ISessionGrain
{
    private readonly IPersistentState<SessionModel> _sessionState;
    private SessionOption _sessionOption;
    
    public SessionGrain(
        [PersistentState("sessions", OrleansIdentityConstants.SESSION_STORAGE_NAME)]IPersistentState<SessionModel> sessionState,
        SessionOption sessionOption)
    {
        _sessionState = sessionState;
        _sessionOption = sessionOption;
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

        _sessionState.State = new SessionModel();

        _sessionState.State.Id = this.GetPrimaryKeyString();
        _sessionState.State.IsActive = true;
        _sessionState.State.UserGrainId = model.UserGrainId;
        _sessionState.State.UserData = model.UserData ?? new();
        _sessionState.State.Status = SessionStatus.Active;
        _sessionState.State.CreatedDate = date;
        _sessionState.State.LastAccess = date;

        await _sessionState.WriteStateAsync();

        var result = GetSessionModel();

        return Result<SessionModel>.Succeed(result);
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

    public ValueTask<Result> AddOrUpdateProperty(string key, string value, bool replace = false)
    {
        if (_sessionState.RecordExists is false)
        {
            DeactivateOnIdle();
            return Result.Fail().AsValueTask();
        }

        if (_sessionState.State.UserData.ContainsKey(key))
        {
            if(replace)
                _sessionState.State.UserData[key] = new HashSet<string> { value };
            else
                _sessionState.State.UserData[key].Add(key);
        }
        else
            _sessionState.State.UserData.Add(key, new HashSet<string> { value });

        return Result.Succeed().AsValueTask();
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
        else
        {
            return Result.Fail().AsValueTask();
        }
    }

    private SessionModel GetSessionModel()
    {
        return new SessionModel
        {
            Id = _sessionState.State.Id,
            IsActive = _sessionState.State.IsActive,
            ClosedDate = _sessionState.State.ClosedDate,
            CreatedDate = _sessionState.State.CreatedDate,
            LastAccess = _sessionState.State.LastAccess,
            Status = _sessionState.State.Status,
            UserData = _sessionState.State.UserData
        };
    }
}