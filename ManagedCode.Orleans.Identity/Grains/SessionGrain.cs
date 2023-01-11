using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using ManagedCode.Communication;
using ManagedCode.Communication.Extensions;
using ManagedCode.Orleans.Identity;
using ManagedCode.Orleans.Identity.Grains.Interfaces;
using ManagedCode.Orleans.Identity.Models;
using ManagedCode.Orleans.Identity.Models.Enums;
using Orleans;
using Orleans.Runtime;


namespace ManagedCode.Orleans.Identity.Grains;

public class SessionGrain : Grain, ISessionGrain
{
    private readonly IPersistentState<SessionModel> _sessionState;
    private readonly SessionModel _sessionEntity;
    
    public SessionGrain([PersistentState("session", "sessionStore")]IPersistentState<SessionModel> sessionState)
    {
        _sessionState = sessionState;
        _sessionEntity = _sessionState.State;
    }

    public Task<Result<SessionModel>> GetSessionAsync()
    {
        return Task.FromResult(Result<SessionModel>.Succeed(_sessionEntity));
    }
    
    public async Task<Result<SessionModel>> CreateAsync(SessionInfo sessionInfo)
    {
        if (_sessionState.State == null)
            _sessionState.State = new SessionModel();
        
        _sessionEntity.Id = Guid.NewGuid().ToString();
        _sessionEntity.Status = SessionStatus.Active;
        _sessionEntity.AppVersion = sessionInfo.AppVersion;
        _sessionEntity.DeviceId = sessionInfo.DeviceId;
        _sessionEntity.DeviceName = sessionInfo.DeviceName;
        _sessionEntity.DevicePushToken = sessionInfo.DevicePushToken;
        _sessionEntity.DevicePlatform = sessionInfo.DevicePlatform;
        _sessionEntity.CreatedDate = DateTime.UtcNow;
        _sessionEntity.LastAccess = DateTime.UtcNow;

        await _sessionState.WriteStateAsync();
        
        return Result<SessionModel>.Succeed(_sessionEntity);
    }
    
    //  private readonly AccountManager _accountManager;
  //  private readonly ISessionRepository _sessionRepository;

    //private SessionEntity _sessionEntity;
    
    // public SessionGrain()
    // {
    //    // _accountManager = userManager;
    //    // _sessionRepository = sessionRepository;
    // }
    
    /*
    public async Task<Result<SessionModel>> CreateAsync(CreateSessionCommand command)
    {
        if (_sessionEntity is not null)
        {
            _sessionEntity.AppVersion = command.AppVersion;
            _sessionEntity.Status = SessionStatus.Active;
            _sessionEntity.LastAccess = DateTime.UtcNow;
            _sessionEntity.DevicePushToken = command.DevicePushToken;
        }
        else
        {
            var account = await _accountManager.FindByPhoneAsync(command.Phone);
            _sessionEntity = new SessionEntity
            {
                Id = this.GetPrimaryKeyString(),
                DeviceId = command.DeviceId,
                AppVersion = command.AppVersion,
                DeviceName = command.DeviceName,
                DevicePlatform = command.DevicePlatform,
                DevicePushToken = command.DevicePushToken,
                CreatedDate = DateTime.UtcNow,
                AccountId = account.Id,
                Status = SessionStatus.Active,
                LastAccess = DateTime.UtcNow
            };
        }

        await _sessionRepository.InsertOrUpdateAsync(_sessionEntity);

        SessionModel sessionModel = new()
        {
            Id = _sessionEntity.Id,
            DeviceId = _sessionEntity.DeviceId,
            AppVersion = _sessionEntity.AppVersion,
            DeviceName = _sessionEntity.DeviceName,
            CreatedDate = _sessionEntity.CreatedDate,
            LastAccess = _sessionEntity.LastAccess,
            ClosedDate = _sessionEntity.ClosedDate,
            Status = SessionStatus.Active,
            AccountId = _sessionEntity.AccountId,
            DevicePushToken = command.DevicePushToken,
            DevicePlatform = command.DevicePlatform,
        };

        return sessionModel;
    }

    Task<Result<KeyValuePair<string, string>[]>> ISessionGrain.ValidateSessionAsync()
    {
        claimsIdentity.AddClaim(new Claim(ClaimTypes.Role, roleName));
        
        throw new NotImplementedException();
    }

    public async Task<Result<SessionModel>> PauseSessionAsync()
    {
        _sessionEntity.LastAccess = DateTime.UtcNow;
        _sessionEntity.Status = SessionStatus.Paused;

        return await GetSessionAsync();
    }

    public async Task<Result<SessionModel>> ResumeSessionAsync()
    {
        _sessionEntity.LastAccess = DateTime.UtcNow;
        _sessionEntity.Status = SessionStatus.Active;

        return await GetSessionAsync();
    }

    
    public async Task<Result<AccountShortInfo>> ValidateSessionAsync()
    {
        var account = await _accountManager.FindByIdAsync(_sessionEntity.AccountId);

        if (account is null)
        {
            return Result<AccountShortInfo>.Fail();
        }

        _sessionEntity.LastAccess = DateTime.UtcNow;

        AccountShortInfo accountShortInfo = new()
        {
            AccountId = account.Id,
            PhoneNumber = account.Phone,
            Email = account.Email,
            Roles = account.Roles,
        };

        return accountShortInfo;
    }

    public async Task<Result> CloseAsync()
    {
        _sessionEntity.Status = SessionStatus.Closed;
        _sessionEntity.ClosedDate = DateTime.UtcNow;

        await SaveStateAsync();

        return Result.Succeed();
    }    

    public override async Task OnActivateAsync(CancellationToken cancellationToken)
    {
        var id = this.GetPrimaryKeyString();
        _sessionEntity = await _sessionRepository.GetAsync(id);
    }

    public override async Task OnDeactivateAsync(DeactivationReason reason, CancellationToken cancellationToken)
    {
        await SaveStateAsync();
    }

    protected async Task SaveStateAsync()
    {
        if (_sessionEntity is not null)
        {
            await _sessionRepository.UpdateAsync(_sessionEntity);
        }
    }
    
    */

    public Task<Result<List<KeyValuePair<string, string>>>> ValidateSessionAsync()
    {
        return Result<List<KeyValuePair<string, string>>>.Fail().AsTask();
    }

    public Task<Result> CloseAsync()
    {
        return Result.Succeed().AsTask();
    }

    public Task<Result<SessionModel>> PauseSessionAsync()
    {
        return Result<SessionModel>.Succeed(new SessionModel()).AsTask();
    }

    public Task<Result<SessionModel>> ResumeSessionAsync()
    {
        return Result<SessionModel>.Succeed(new SessionModel()).AsTask();
    }

    public Task<Result> AddRole(string role)
    {
        return Result.Succeed().AsTask();
    }

    public Task<Result> AddClaim(Claim claim)
    {
        return Result.Succeed().AsTask();
    }

    public Task<Result> RemoveRole(string role)
    {
        return Result.Fail().AsTask();
    }

    public Task<Result> RemoveClaim(Claim claim)
    {
        return Result.Fail().AsTask();
    }
}