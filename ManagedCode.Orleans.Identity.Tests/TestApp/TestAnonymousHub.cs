using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace ManagedCode.Orleans.Identity.Tests.TestApp;

[AllowAnonymous]
public class TestAnonymousHub : Hub
{
    public Task<int> DoTest()
    {
        return Task.FromResult(new Random().Next());
    }
}