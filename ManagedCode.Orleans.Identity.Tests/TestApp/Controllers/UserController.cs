using ManagedCode.Orleans.Identity.Core.Extensions;
using ManagedCode.Orleans.Identity.Tests.Cluster.Grains;
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