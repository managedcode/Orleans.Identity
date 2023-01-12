using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ManagedCode.Communication;
using ManagedCode.Orleans.Identity.Entities;
using ManagedCode.Orleans.Identity.Grains.Interfaces;
using ManagedCode.Orleans.Identity.Models;
using ManagedCode.Orleans.Identity.Options;
using ManagedCode.Orleans.Identity.Shared.Enums;
using Orleans;
using Orleans.Runtime;

namespace ManagedCode.Orleans.Identity.Grains;

public class SessionGrain : Grain, ISessionGrain
{
    private readonly IPersistentState<SessionEntity> _sessionState;
    private SessionOption _sessionOption;
    
    public SessionGrain(
        [PersistentState("session", "sessionStore")]IPersistentState<SessionEntity> sessionState,
        SessionOption sessionOption)
    {
        _sessionState = sessionState;
        _sessionOption = sessionOption;
    }

    public override Task OnActivateAsync(CancellationToken cancellationToken)
    {
        return base.OnActivateAsync(cancellationToken);
    }

    public override async Task OnDeactivateAsync(DeactivationReason reason, CancellationToken cancellationToken)
    {
        //TODO: check is exist
        await _sessionState.WriteStateAsync();
    }

    public Task<Result<SessionModel>> GetSessionAsync()
    {
        if (!_sessionState.RecordExists)
        {
            return Task.FromResult(Result<SessionModel>.Fail());
        }

        var result = GetSessionModel();

        return Task.FromResult(Result<SessionModel>.Succeed(result));
    }
    
    public async Task<Result<SessionModel>> CreateAsync(CreateSessionModel model)
    {
        if (!_sessionState.RecordExists)
            _sessionState.State = new SessionEntity();

        _sessionState.State.IsActive = true;
        _sessionState.State.UserGrainId = model.UserGrainId;
        _sessionState.State.UserData = model.UserData ?? new();
        _sessionState.State.Status = SessionStatus.Active;
        _sessionState.State.CreatedDate = DateTime.UtcNow;
        _sessionState.State.LastAccess = DateTime.UtcNow;

        await _sessionState.WriteStateAsync();

        var result = GetSessionModel();

        return Result<SessionModel>.Succeed(result);
    }

    public ValueTask<Result<Dictionary<string, string>>> ValidateAndGetClaimsAsync()
    {
        if (!_sessionState.RecordExists)
        {
            DeactivateOnIdle();
            return Result<Dictionary<string, string>>.Fail().AsValueTask();
        }

        if (!_sessionState.State.IsActive)
        {
            return Result<Dictionary<string, string>>.Fail().AsValueTask();
        }
            
        _sessionState.State.LastAccess = DateTime.UtcNow;

        var result = new Dictionary<string, string>();

        foreach (var item in _sessionState.State.UserData)
        {
            result.Add(item.Key, item.Value);
        }

        return Result<Dictionary<string, string>>.Succeed(result).AsValueTask();
    }

    public async Task<Result> CloseAsync()
    {
        if (!_sessionState.RecordExists)
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
        if (!_sessionState.RecordExists)
        {
            return Result.Fail().AsValueTask();
        }

        _sessionState.State.LastAccess = DateTime.UtcNow;
        _sessionState.State.Status = SessionStatus.Paused;

        return Result.Succeed().AsValueTask();
    }

    public ValueTask<Result> ResumeSessionAsync()
    {
        if (!_sessionState.RecordExists)
        {
            return Result.Fail().AsValueTask();
        }

        _sessionState.State.LastAccess = DateTime.UtcNow;
        _sessionState.State.Status = SessionStatus.Active;

        return Result.Succeed().AsValueTask();
    }

    public ValueTask<Result> AddProperty(string key, string value)
    {
        if (_sessionState.State is null)
        {
            return Result.Fail().AsValueTask();
        }

        if (_sessionState.State.UserData.ContainsKey(key))
        {
            return Result.Fail(ResultStatus.PropertyWithThisKeyAlreadyExists).AsValueTask();
        }

        _sessionState.State.UserData.Add(key, value);

        return Result.Succeed().AsValueTask();
    }

    public ValueTask<Result> RemoveProperty(string key)
    {
        if (_sessionState.State is null)
        {
            return Result.Fail().AsValueTask();
        }

        if (!_sessionState.State.UserData.ContainsKey(key))
        {
            return Result.Fail(ResultStatus.PropertyWithThisKeyDoesNotExist).AsValueTask();
        }

        _sessionState.State.UserData.Remove(key);

        return Result.Succeed().AsValueTask();
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
            Status = _sessionState.State.Status
        };
    }
}