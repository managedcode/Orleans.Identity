using ManagedCode.Orleans.Identity.Core.Extensions;
using ManagedCode.Orleans.Identity.Tests.Cluster.Grains.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ManagedCode.Orleans.Identity.Tests.TestApp.Controllers;

[AllowAnonymous]
[Route("publicController")]
public class PublicController : ControllerBase
{
    private readonly IClusterClient _clusterClient;

    public PublicController(IClusterClient clusterClient)
    {
        _clusterClient = clusterClient;
    }
    
    [HttpGet]
    public async Task<ActionResult<string>> CallCommonMethod()
    {
        var userId = User.GetGrainId();
        var publicGrain = _clusterClient.GetGrain<IPublicGrain>(userId);
        return await publicGrain.CommonMethod();
    }

    [HttpGet("authorizedMethod")]
    public async Task<ActionResult<string>> CallAuthorizedMethod()
    {
        var userId = User.GetGrainId();
        var publicGrain = _clusterClient.GetGrain<IPublicGrain>(userId);
        return await publicGrain.AuthorizedMethod();
    }

    [HttpGet("adminMethod")]
    public async Task<ActionResult<string>> CallAdminMethod()
    {
        var userId = User.GetGrainId();
        var publicGrain = _clusterClient.GetGrain<IPublicGrain>(userId);
        return await publicGrain.AdminOnly();   
    }

    [HttpGet("moderatorMethod")]
    public async Task<ActionResult<string>> CallModeratorMethod()
    {
        var userId = User.GetGrainId();
        var publicGrain = _clusterClient.GetGrain<IPublicGrain>(userId);
        return await publicGrain.ModeratorOnly();
    }
}