using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using ManagedCode.Communication;
using ManagedCode.Orleans.Identity.Models;
using Orleans;

namespace ManagedCode.Orleans.Identity.Grains.Interfaces;

public interface ISessionGrain : IGrainWithStringKey
{
    Task<Result<SessionModel>> CreateAsync(CreateSessionModal sessionInfo);

   Task<Result<List<KeyValuePair<string, string>>>> ValidateSessionAsync();

    Task<Result<SessionModel>> GetSessionAsync();
    
    Task<Result> CloseAsync();
    Task<Result<SessionModel>> PauseSessionAsync();
    Task<Result<SessionModel>> ResumeSessionAsync();
    
    
    Task<Result> AddRole(string role);
    Task<Result> AddClaim(Claim claim);
    
    Task<Result> RemoveRole(string role);
    Task<Result> RemoveClaim(Claim claim);

}