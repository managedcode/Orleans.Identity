using System.Security.Claims;
using ManagedCode.Orleans.Identity.Core.Extensions;
using ManagedCode.Orleans.Identity.Tests.Cluster.Grains;
using ManagedCode.Orleans.Identity.Tests.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ManagedCode.Orleans.Identity.Tests.TestApp.Controllers;

[Authorize]
[Route("userController")]
public class UserController : ControllerBase
{
    private readonly IClusterClient _clusterClient;

    public UserController(IClusterClient clusterClient)
    {
        _clusterClient = clusterClient;
    }

    [HttpGet]
    public async Task<ActionResult<string>> GetUser()
    {
        try
        {
            var userId = User.GetGrainId();
            var userGrain = _clusterClient.GetGrain<IUserGrain>(userId);
            return await userGrain.GetUser();
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
    }

    [HttpGet("stream")]
    public IAsyncEnumerable<string> ReadStream() =>
        _clusterClient.GetGrain<IStreamingAuthorizationGrain>(Guid.NewGuid()).InterfaceProtected("http");

    [HttpGet("stream-downgraded")]
    public IAsyncEnumerable<string> ReadStreamWithReducedPrivileges()
    {
        HttpContext.User = new ClaimsPrincipal(new ClaimsIdentity(
            [new Claim(ClaimTypes.Name, "delegated"), new Claim(ClaimTypes.Role, TestRoles.USER)], "delegation"));
        return ReadAdminInfoAsync();
    }

    private async IAsyncEnumerable<string> ReadAdminInfoAsync()
    {
        string result;
        try
        {
            result = await _clusterClient.GetGrain<IUserGrain>("stream-delegation").GetAdminInfo();
        }
        catch (UnauthorizedAccessException)
        {
            result = "denied";
        }

        yield return result;
    }

    [HttpGet("anonymous")]
    [AllowAnonymous]
    public async Task<ActionResult<string>> TryGetUser()
    {
        try
        {
            var result = await GetUser();
            return result;
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
    }

    [HttpGet("ban")]
    public async Task<ActionResult<string>> BanUser()
    {
        try
        {
            var userId = User.GetGrainId();
            var userGrain = _clusterClient.GetGrain<IUserGrain>(userId);
            return await userGrain.BanUser();
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
    }

    [HttpGet("publicInfo")]
    [AllowAnonymous]
    public async Task<ActionResult<string>> GetPublicInfo()
    {
        try
        {
            // For public endpoints, use a unique grain ID for unauthenticated users
            var userId = User.Identity?.IsAuthenticated == true ? User.GetGrainId() : Guid.NewGuid().ToString();

            var userGrain = _clusterClient.GetGrain<IUserGrain>(userId);
            var result = await userGrain.GetPublicInfo();
            return result;
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
    }

    [HttpGet("modify")]
    public async Task<ActionResult<string>> ModifyUser()
    {
        try
        {
            var userId = User.GetGrainId();
            var userGrain = _clusterClient.GetGrain<IUserGrain>(userId);
            var result = await userGrain.ModifyUser();
            return result;
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
    }

    [HttpGet("addToList")]
    [AllowAnonymous]
    public async Task<ActionResult<string>> AddToList()
    {
        try
        {
            // For anonymous endpoints, generate a unique grain ID for unauthenticated users
            var userId = User.Identity?.IsAuthenticated == true ? User.GetGrainId() : Guid.NewGuid().ToString();

            var userGrain = _clusterClient.GetGrain<IUserGrain>(userId);
            return await userGrain.AddToList();
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
    }
}