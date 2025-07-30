using ManagedCode.Orleans.Identity.Tests.Cluster.Grains;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Orleans;

namespace ManagedCode.Orleans.Identity.Tests.TestApp;

[Authorize]
public class TestAuthorizeHub : Hub
{
    private readonly IClusterClient _clusterClient;

    public TestAuthorizeHub(IClusterClient clusterClient)
    {
        _clusterClient = clusterClient;
    }

    public async Task<string> GetUserInfo()
    {
        try
        {
            var userId = Context.User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "anonymous";
            var userGrain = _clusterClient.GetGrain<IUserGrain>(userId);
            return await userGrain.GetUser();
        }
        catch (UnauthorizedAccessException)
        {
            throw new HubException("Forbidden: Access denied");
        }
    }

    [Authorize(Roles = "admin")]
    public async Task<string> GetAdminInfo()
    {
        var userId = Context.User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "anonymous";
        var userGrain = _clusterClient.GetGrain<IUserGrain>(userId);
        return await userGrain.GetAdminInfo();
    }

    public async Task<string> GetPublicInfo()
    {
        try
        {
            var userId = Context.User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "anonymous";
            var userGrain = _clusterClient.GetGrain<IUserGrain>(userId);
            return await userGrain.GetPublicInfo();
        }
        catch (UnauthorizedAccessException)
        {
            throw new HubException("Forbidden: Access denied");
        }
    }
}