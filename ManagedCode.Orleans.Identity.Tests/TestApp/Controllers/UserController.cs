using ManagedCode.Orleans.Identity.Middlewares;
using ManagedCode.Orleans.Identity.Tests.Cluster.Grains.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ManagedCode.Orleans.Identity.Tests.Cluster;

[Route("userController")]
[Authorize]
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
        var userId = User.GetGrainId();
        var userGrain = _clusterClient.GetGrain<IUserGrain>(userId);
        return await userGrain.GetUser();
    }

    [HttpGet("anonymous")]
    [AllowAnonymous]
    public async Task<ActionResult<string>> TryGetUser()
    {
        var result = await GetUser();
        return result;
    }

    [HttpGet("ban")]
    public async Task<ActionResult<string>> BanUser()
    {
        var userId = User.GetGrainId();
        var userGrain = _clusterClient.GetGrain<IUserGrain>(userId);
        return await userGrain.BanUser();
    }

    [HttpGet("publicInfo")]
    [AllowAnonymous]
    public async Task<ActionResult<string>> GetPublicInfo()
    {
        var userId = User.GetGrainId();
        var userGrain = _clusterClient.GetGrain<IUserGrain>(userId);
        var result = await userGrain.GetPublicInfo();
        return result;
    }

    [HttpGet("modify")]
    public async Task<ActionResult<string>> ModifyUser()
    {
        var userId = User.GetGrainId();
        var userGrain = _clusterClient.GetGrain<IUserGrain>(userId);
        var result = await userGrain.ModifyUser();
        return result;
    }

    [HttpGet("addToList")]
    [AllowAnonymous]
    public async Task<ActionResult<string>> AddToList()
    {
        var userId = User.GetGrainId();
        var userGrain = _clusterClient.GetGrain<IUserGrain>(userId);
        return await userGrain.AddToList();
    }
}