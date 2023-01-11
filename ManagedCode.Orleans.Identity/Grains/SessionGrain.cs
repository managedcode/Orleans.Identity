using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using ManagedCode.Communication;
using ManagedCode.Orleans.Identity.Entities;
using ManagedCode.Orleans.Identity.Grains.Interfaces;
using ManagedCode.Orleans.Identity.Models;
using ManagedCode.Orleans.Identity.Shared.Enums;
using Orleans;
using Orleans.Runtime;

namespace ManagedCode.Orleans.Identity.Grains;

public class SessionGrain : Grain, ISessionGrain
{
    private readonly IPersistentState<SessionEntity> _sessionState;
    
    public SessionGrain([PersistentState("session", "sessionStore")]IPersistentState<SessionEntity> sessionState)
    {
        _sessionState = sessionState;
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
        var result = GetSessionModel();

        return Task.FromResult(Result<SessionModel>.Succeed(result));
    }
    
    public async Task<Result<SessionModel>> CreateAsync(CreateSessionModel model)
    {
        if (_sessionState.State == null)
            _sessionState.State = new SessionEntity();

        _sessionState.State.IsActive = true;
        _sessionState.State.UserGrainId = model.UserGrainId;
        _sessionState.State.UserData = model.UserData;
        _sessionState.State.Status = SessionStatus.Active;
        _sessionState.State.CreatedDate = DateTime.UtcNow;
        _sessionState.State.LastAccess = DateTime.UtcNow;

        await _sessionState.WriteStateAsync();

        var result = GetSessionModel();

        return Result<SessionModel>.Succeed(result);
    }

    public ValueTask<Result<Dictionary<string, string>>> ValidateAndGetClaimsAsync()
    {
        if (_sessionState.State is null)
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
        if (_sessionState.State is null)
        {
            return Result.Fail();
        }

        _sessionState.State.ClosedDate = DateTime.UtcNow;
        _sessionState.State.Status = SessionStatus.Closed;
        _sessionState.State.IsActive = false;
        //_sessionState.ClearStateAsync(); //TODO: clear state
        await _sessionState.WriteStateAsync();

        return Result.Succeed();

    }

    public ValueTask<Result> PauseSessionAsync()
    {
        if (_sessionState.State is null)
        {
            return Result.Fail().AsValueTask();
        }

        _sessionState.State.LastAccess = DateTime.UtcNow;
        _sessionState.State.Status = SessionStatus.Paused;

        return Result.Succeed().AsValueTask();
    }

    public ValueTask<Result> ResumeSessionAsync()
    {
        if (_sessionState.State is null)
        {
            return Result.Fail().AsValueTask();
        }

        _sessionState.State.LastAccess = DateTime.UtcNow;
        _sessionState.State.Status = SessionStatus.Active;

        return Result.Succeed().AsValueTask();
    }

    public ValueTask<Result> AddRoleAsync(string role)
    {
        var claimsPrincipal = ClaimsPrincipal.Current;

        if (claimsPrincipal!.IsInRole(role))
        {
            return Result.Fail(ResultStatus.ThereIsSuchRoleAlready).AsValueTask();
        }

        var identity = claimsPrincipal.Identity as ClaimsIdentity;
        identity!.AddClaim(new Claim(ClaimTypes.Role, role));

        return Result.Succeed().AsValueTask();
    }

    public ValueTask<Result> AddClaim(Claim claim)
    {
        var claimsIdentity = ClaimsPrincipal.Current!.Identity as ClaimsIdentity;

        claimsIdentity!.AddClaim(claim);

        return Result.Succeed().AsValueTask();
    }

    public ValueTask<Result> RemoveRole(string role)
    {
        var claimsPrincipal = ClaimsPrincipal.Current;

        if (!claimsPrincipal!.IsInRole(role))
        {
            return Result.Fail(ResultStatus.SuchRoleDoesNotExist).AsValueTask();
        }

        var identity = claimsPrincipal.Identity as ClaimsIdentity;
        identity!.RemoveClaim(new Claim(ClaimTypes.Role, role));

        return Result.Succeed().AsValueTask();
    }

    public ValueTask<Result> RemoveClaim(Claim claim)
    {
        var claimsIdentity = ClaimsPrincipal.Current!.Identity as ClaimsIdentity;

        claimsIdentity!.RemoveClaim(claim);

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