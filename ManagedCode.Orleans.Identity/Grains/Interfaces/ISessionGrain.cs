using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using ManagedCode.Communication;
using ManagedCode.Orleans.Identity.Models;
using Orleans;

namespace ManagedCode.Orleans.Identity.Grains.Interfaces;

public interface ISessionGrain : IGrainWithStringKey
{
    Task<Result<SessionModel>> CreateAsync(CreateSessionModel sessionInfo);
    Task<Result> CloseAsync();
    Task<Result<SessionModel>> GetSessionAsync();
    ValueTask<Result<Dictionary<string, string>>> ValidateAndGetClaimsAsync();

    ValueTask<Result> PauseSessionAsync();
    ValueTask<Result> ResumeSessionAsync();

    ValueTask<Result> AddRoleAsync(string role);
    ValueTask<Result> AddClaim(Claim claim);

    ValueTask<Result> RemoveRole(string role);
    ValueTask<Result> RemoveClaim(Claim claim);

}