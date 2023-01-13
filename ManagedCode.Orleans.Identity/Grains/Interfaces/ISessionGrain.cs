using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading.Tasks;
using ManagedCode.Communication;
using ManagedCode.Orleans.Identity.Models;
using Orleans;

namespace ManagedCode.Orleans.Identity.Grains.Interfaces;

public interface ISessionGrain : IGrainWithStringKey
{
    Task<Result<SessionModel>> CreateAsync(CreateSessionModel sessionInfo);
    Task<Result> CloseAsync();
    ValueTask<Result<SessionModel>> GetSessionAsync();
    ValueTask<Result<ImmutableDictionary<string, HashSet<string>>>> ValidateAndGetClaimsAsync();

    ValueTask<Result> PauseSessionAsync();
    ValueTask<Result> ResumeSessionAsync();

    ValueTask<Result> AddOrUpdateProperty(string key, string value, bool replace = false);
    ValueTask<Result> AddOrUpdateProperty(string key, List<string> values, bool replace = false);

    ValueTask<Result> RemoveProperty(string key);
    //ValueTask<Result> RemoveValueFromProperty(string key, string value);
    //ValueTask<Result> ClearUserData();
}