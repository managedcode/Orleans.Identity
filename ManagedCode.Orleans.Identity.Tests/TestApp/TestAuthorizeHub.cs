using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Threading.Channels;
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

    [Authorize]
    public IAsyncEnumerable<string> ReadStream() =>
        clusterClient.GetGrain<IStreamingAuthorizationGrain>(Guid.NewGuid()).InterfaceProtected("hub");

    [Authorize]
    public Task<IAsyncEnumerable<string>> ReadStreamAsync() => Task.FromResult(ReadStream());

    [Authorize]
    public ChannelReader<string> ReadChannel() => new GrainChannelReader(clusterClient.GetGrain<IUserGrain>("channel"));

    [Authorize]
    public Task<ChannelReader<string>> ReadChannelAsync() => Task.FromResult(ReadChannel());

    [Authorize]
    public ChannelStreamTask ReadChannelTask()
    {
        var task = new ChannelStreamTask(ReadChannel());
        task.Start(TaskScheduler.Default);
        return task;
    }

    [Authorize]
    public ChannelStreamAwaitable ReadChannelAwaitable() => new(ReadChannel());

    public readonly struct ChannelStreamAwaitable(ChannelReader<string> reader)
    {
        public ValueTaskAwaiter<ChannelReader<string>> GetAwaiter() =>
            new ValueTask<ChannelReader<string>>(reader).GetAwaiter();
    }

    public sealed class ChannelStreamTask(ChannelReader<string> reader) : Task<ChannelReader<string>>(() => reader);

    private sealed class GrainChannelReader(IUserGrain grain) : ChannelReader<string>
    {
        private string? pending;
        private bool fetched;

        public override async ValueTask<bool> WaitToReadAsync(CancellationToken cancellationToken = default)
        {
            if (!fetched)
            {
                pending = await grain.GetUser().WaitAsync(cancellationToken);
                fetched = true;
            }

            return pending is not null;
        }

        public override bool TryRead([MaybeNullWhen(false)] out string item)
        {
            item = pending;
            pending = null;
            return item is not null;
        }
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