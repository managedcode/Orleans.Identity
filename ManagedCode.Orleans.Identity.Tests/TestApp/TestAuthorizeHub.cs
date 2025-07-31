using ManagedCode.Orleans.Identity.Tests.Cluster.Grains;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Orleans;

namespace ManagedCode.Orleans.Identity.Tests.TestApp;

public class TestAuthorizeHub(IClusterClient clusterClient) : Hub
{
    [Authorize]
    public async Task<string> GetUserInfo()
    {
        try
        {
            var userId = Context.User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ??
                         "anonymous";
            var userGrain = clusterClient.GetGrain<IUserGrain>(userId);
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
        var userGrain = clusterClient.GetGrain<IUserGrain>(userId);
        return await userGrain.GetAdminInfo();
    }

    [AllowAnonymous]
    public async Task<string> GetPublicInfo()
    {
        try
        {
            var userId = Context.User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ??
                         "anonymous";
            var userGrain = clusterClient.GetGrain<IUserGrain>(userId);
            return await userGrain.GetPublicInfo();
        }
        catch (UnauthorizedAccessException)
        {
            throw new HubException("Forbidden: Access denied");
        }
    }

    [Authorize]
    public async Task SendAuthorizedMessage(string message)
    {
        try
        {
            var userId = Context.User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ??
                         "anonymous";
            var userGrain = clusterClient.GetGrain<IUserGrain>(userId);

            // Call grain method to get user info (this will trigger authorization filter)
            var userInfo = await userGrain.GetUser();

            // Send message back to caller
            await Clients.Caller.SendAsync("ReceiveMessage", $"{userInfo} received authorized message: {message}");
        }
        catch (UnauthorizedAccessException)
        {
            throw new HubException("Forbidden: Access denied");
        }
    }
}