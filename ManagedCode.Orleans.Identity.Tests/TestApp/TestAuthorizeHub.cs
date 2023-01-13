using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace ManagedCode.Orleans.Identity.Tests.Cluster;

[Authorize]
public class TestAuthorizeHub : Hub
{
    public Task<int> DoTest()
    {
        return Task.FromResult(new Random().Next());
    }
}