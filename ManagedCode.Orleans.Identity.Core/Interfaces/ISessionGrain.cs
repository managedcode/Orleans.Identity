using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading.Tasks;
using ManagedCode.Communication;
using ManagedCode.Orleans.Identity.Core.Models;
using Orleans;

namespace ManagedCode.Orleans.Identity.Core.Interfaces;

public interface ISessionGrain : IGrainWithStringKey
{
    /// <summary>
    ///  Creates new session and save into storage
    /// </summary>
    /// <param name="sessionInfo"> Session create model to create </param>
    /// <returns> Created session model </returns>
    
    Task<Result<SessionModel>> CreateAsync(CreateSessionModel sessionInfo);
    /// <summary>
    /// Closes session, if ClearStateOnClose in SessionOptions is true, will delete session from storage. If session doesn't exist, deactivates grain
    /// </summary>
    /// <returns>Information about method execution</returns>
    
    Task<Result> CloseAsync();
    /// <summary>
    /// Get session from storage
    /// </summary>
    /// <returns>Session that associated with grain id</returns>
    
    ValueTask<Result<SessionModel>> GetSessionAsync();
    /// <summary>
    ///  Check if session exists and is active, if active returns session's user data. If session doesn't exist or not active, deactivates grain
    /// </summary>
    /// <returns>If session found, returns session's user data, if not returns failed result </returns>
    
    ValueTask<Result<ImmutableDictionary<string, HashSet<string>>>> ValidateAndGetClaimsAsync();
    /// <summary>
    /// Sets session's status to paused. If session doesn't exist, deactivates grain
    /// </summary>
    /// <returns>Information about method execution</returns>
    
    ValueTask<Result> PauseSessionAsync();
    /// <summary>
    /// Sets session's status to active. If session doesn't exist, deactivates grain
    /// </summary>
    /// <returns>Information about method execution</returns>
    
    ValueTask<Result> ResumeSessionAsync();
    /// <summary>
    /// Add key-value pair to session's user data, if there already data with same key, returns fail. If session doesn't exist, deactivates grain
    /// </summary>
    /// <param name="key">Key that associated with property's value</param>
    /// <param name="value">Property's value, represented as string</param>
    /// <returns>Information about method execution</returns>
    ValueTask<Result> AddProperty(string key, string value);
    
    /// <summary>
    /// Add key-value pair to session's user data, removes duplicates from list of values, if there already data with same key, returns fail. If session doesn't exist, deactivates grain
    /// </summary>
    /// <param name="key">Key that associated with property's value</param>
    /// <param name="values">Unique property's values, represented as string </param>
    /// <returns>Information about method execution</returns>
    ValueTask<Result> AddProperty(string key, List<string> values);

    /// <summary>
    /// If session has user data with specified key, then replaces property's value. If session doesn't exist, deactivates grain
    /// </summary>
    /// <param name="key">Key of required property</param>
    /// <param name="value">New value to replace old property's value</param>
    /// <returns></returns>
    ValueTask<Result> ReplaceProperty(string key, string value);
    
    /// <summary>
    /// If session has user data with specified key, then replaces property's value. If session doesn't exist, deactivates grain
    /// </summary>
    /// <param name="key">Key of required property</param>
    /// <param name="values">New values to replace old property's values</param>
    /// <returns></returns>
    ValueTask<Result> ReplaceProperty(string key, List<string> values);

    /// <summary>
    /// Removes property associated with key from session's user data. If session doesn't exist, deactivates grain
    /// </summary>
    /// <param name="key">Property's key to remove</param>
    /// <returns>If property exists and was removed returns <c>Result.Succeed()</c>, if session or property doesn't exist returns <c>Result.Fail()</c></returns>
    ValueTask<Result> RemoveProperty(string key);

    /// <summary>
    /// Removes specified value from session's user data that associated specified key. If session doesn't exist, deactivates grain
    /// </summary>
    /// <param name="key">Property's key</param>
    /// <param name="value">Value to remove from property</param>
    /// <returns>Information about method execution</returns>
    ValueTask<Result> RemoveValueFromProperty(string key, string value);

    /// <summary>
    /// Add specified value to session's user data that associated specified key. If session doesn't exist, deactivates grain
    /// </summary>
    /// <param name="key">Property's key</param>
    /// <param name="value">Value to add to property</param>
    /// <returns>Information about method execution</returns>
    ValueTask<Result> AddValueToProperty(string key, string value);

    /// <summary>
    /// Delete all session's user data in <c>SessionModel.UserData. If session doesn't exist, deactivates grain</c>
    /// </summary>
    /// <returns>Information about method execution</returns>
    ValueTask<Result> ClearUserData();
}