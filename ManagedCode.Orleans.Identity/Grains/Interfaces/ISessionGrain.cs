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

    ValueTask<Result> Add(string key, string value);
    ValueTask<Result> Add(string key, List<string> values);
    
    ValueTask<Result> ReplaceProperty(string key, string value);
    ValueTask<Result> ReplaceProperty(string key, List<string> values);

    ValueTask<Result> Update(string key, string value);
    ValueTask<Result> Update(string key, List<string> value);
    
    ValueTask<Result> RemoveProperty(string key);
    ValueTask<Result> RemoveValueFromProperty(string key, string value);
    
    ValueTask<Result> ClearUserData();
}