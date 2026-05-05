using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace ManagedCode.Orleans.Identity.Tests.TestApp;

[AllowAnonymous]
public class TestAnonymousHub : Hub
{
    public Task<int> DoTest()
    {
        _ = Context.ConnectionId;
        return Task.FromResult(Random.Shared.Next());
    }
}
