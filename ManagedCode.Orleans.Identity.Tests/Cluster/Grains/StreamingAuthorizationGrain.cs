using System.Runtime.CompilerServices;
using ManagedCode.Orleans.Identity.Core.Attributes;
using ManagedCode.Orleans.Identity.Tests.Constants;
using ManagedCode.Orleans.Identity.Server.GrainCallFilter;
using Orleans.Concurrency;
using Orleans.Runtime;
using Microsoft.AspNetCore.Authorization;

namespace ManagedCode.Orleans.Identity.Tests.Cluster.Grains;

public interface IStreamingAuthorizationGrain : IGrainWithGuidKey
{
    [Authorize(Roles = TestRoles.ADMIN)]
    IAsyncEnumerable<string> InterfaceProtected(string value, CancellationToken cancellationToken = default);
    IAsyncEnumerable<string> ImplementationProtected(string value);
    [Authorize(Policy = StreamingAuthorizationGrain.ResourcePolicy)]
    IAsyncEnumerable<string> ResourceProtected(string value);
    [AllowAnonymous]
    IAsyncEnumerable<string> ReadPublic();
    [Authorize(AuthenticationSchemes = "unsupported")]
    IAsyncEnumerable<string> UnsupportedScheme();
    [Authorize(Roles = TestRoles.ADMIN)]
    IAsyncEnumerable<T> GenericProtected<T>(T value);
    [AllowAnonymous]
    Task<(int Started, int Disposed)> Counts();
    [Authorize(Roles = TestRoles.ADMIN)]
    IAsyncEnumerable<string> BlockAfterFirst();
    [AllowAnonymous]
    [AlwaysInterleave]
    Task WaitForBlockedMoveNext();
    [AllowAnonymous]
    [AlwaysInterleave]
    Task ReleaseBlockedMoveNext();
    [AllowAnonymous]
    [AlwaysInterleave]
    Task<bool> IsDisposing(Guid enumerationId);
}

public sealed class StreamingAuthorizationGrain : Grain, IStreamingAuthorizationGrain
{
    public const string ResourcePolicy = "stream-original-resource";
    private int started;
    private int disposed;
    private readonly TaskCompletionSource blockedMoveNextStarted = new(TaskCreationOptions.RunContinuationsAsynchronously);
    private readonly TaskCompletionSource blockedMoveNextRelease = new(TaskCreationOptions.RunContinuationsAsynchronously);

    public IAsyncEnumerable<string> InterfaceProtected(string value, CancellationToken cancellationToken = default) =>
        Enumerate(value, cancellationToken);
    [Authorize(Roles = TestRoles.ADMIN)]
    public IAsyncEnumerable<string> ImplementationProtected(string value) => Enumerate(value);
    public IAsyncEnumerable<string> ResourceProtected(string value) => Enumerate(value);
    public IAsyncEnumerable<string> ReadPublic() => Enumerate("public");
    public IAsyncEnumerable<string> UnsupportedScheme() => Enumerate("unsupported");
    public async IAsyncEnumerable<T> GenericProtected<T>(T value)
    {
        yield return value;
        await Task.Yield();
    }
    public Task<(int Started, int Disposed)> Counts() => Task.FromResult((started, disposed));
    public Task WaitForBlockedMoveNext() => blockedMoveNextStarted.Task;
    public Task ReleaseBlockedMoveNext()
    {
        blockedMoveNextRelease.TrySetResult();
        return Task.CompletedTask;
    }
    public Task<bool> IsDisposing(Guid enumerationId) => Task.FromResult(
        this.GrainContext.GetComponent<StreamingAuthorizationState>()?.Find(enumerationId)?.Disposing == true);
    public async IAsyncEnumerable<string> BlockAfterFirst()
    {
        started++;
        try
        {
            yield return "first";
            blockedMoveNextStarted.TrySetResult();
            await blockedMoveNextRelease.Task;
        }
        finally
        {
            disposed++;
        }
    }

    private async IAsyncEnumerable<string> Enumerate(string value,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        started++;
        try
        {
            for (var index = 0; index < 3; index++)
            {
                cancellationToken.ThrowIfCancellationRequested();
                await Task.Yield();
                yield return value + index;
            }
        }
        finally
        {
            disposed++;
        }
    }
}

public interface IClassProtectedStreamingGrain : IGrainWithGuidKey
{
    IAsyncEnumerable<int> Read();
}

[Authorize(Roles = TestRoles.ADMIN)]
public sealed class ClassProtectedStreamingGrain : Grain, IClassProtectedStreamingGrain
{
    public async IAsyncEnumerable<int> Read()
    {
        yield return 1;
        await Task.Yield();
    }
}

[GrainAllowAnonymous]
public interface IInterfaceAnonymousAuthorizationGrain : IGrainWithGuidKey
{
    [Authorize(Roles = TestRoles.ADMIN)]
    Task<string> TaskInterfaceProtected();
    Task<string> TaskImplementationProtected();
    Task<string> TaskDefault();
    [Authorize(Roles = TestRoles.ADMIN)]
    Task<string> TaskPublicOverride();
    [Authorize(Roles = TestRoles.ADMIN)]
    IAsyncEnumerable<string> StreamInterfaceProtected();
    IAsyncEnumerable<string> StreamImplementationProtected();
    IAsyncEnumerable<string> StreamDefault();
    [Authorize(Roles = TestRoles.ADMIN)]
    IAsyncEnumerable<string> StreamPublicOverride();
    Task<(int Tasks, int Streams)> Counts();
}

public interface IClassAnonymousAuthorizationGrain : IGrainWithGuidKey
{
    [Authorize(Roles = TestRoles.ADMIN)]
    Task<string> TaskInterfaceProtected();
    Task<string> TaskImplementationProtected();
    Task<string> TaskDefault();
    [Authorize(Roles = TestRoles.ADMIN)]
    Task<string> TaskPublicOverride();
    [Authorize(Roles = TestRoles.ADMIN)]
    IAsyncEnumerable<string> StreamInterfaceProtected();
    IAsyncEnumerable<string> StreamImplementationProtected();
    IAsyncEnumerable<string> StreamDefault();
    [Authorize(Roles = TestRoles.ADMIN)]
    IAsyncEnumerable<string> StreamPublicOverride();
    Task<(int Tasks, int Streams)> Counts();
}

public sealed class InterfaceAnonymousAuthorizationGrain : Grain, IInterfaceAnonymousAuthorizationGrain
{
    private int tasks;
    private int streams;

    public Task<string> TaskInterfaceProtected() => ReadTask("secret");
    [Authorize(Roles = TestRoles.ADMIN)]
    public Task<string> TaskImplementationProtected() => ReadTask("secret");
    public Task<string> TaskDefault() => ReadTask("default");
    [AllowAnonymous]
    public Task<string> TaskPublicOverride() => ReadTask("public");
    public IAsyncEnumerable<string> StreamInterfaceProtected() => ReadStream("secret");
    [Authorize(Roles = TestRoles.ADMIN)]
    public IAsyncEnumerable<string> StreamImplementationProtected() => ReadStream("secret");
    public IAsyncEnumerable<string> StreamDefault() => ReadStream("default");
    [AllowAnonymous]
    public IAsyncEnumerable<string> StreamPublicOverride() => ReadStream("public");
    public Task<(int Tasks, int Streams)> Counts() => Task.FromResult((tasks, streams));

    private Task<string> ReadTask(string value)
    {
        tasks++;
        return Task.FromResult(value);
    }

    private async IAsyncEnumerable<string> ReadStream(string value)
    {
        streams++;
        yield return value;
        await Task.Yield();
        yield return value;
    }
}

[AllowAnonymous]
public sealed class ClassAnonymousAuthorizationGrain : Grain, IClassAnonymousAuthorizationGrain
{
    private int tasks;
    private int streams;

    public Task<string> TaskInterfaceProtected() => ReadTask("secret");
    [Authorize(Roles = TestRoles.ADMIN)]
    public Task<string> TaskImplementationProtected() => ReadTask("secret");
    public Task<string> TaskDefault() => ReadTask("default");
    [AllowAnonymous]
    public Task<string> TaskPublicOverride() => ReadTask("public");
    public IAsyncEnumerable<string> StreamInterfaceProtected() => ReadStream("secret");
    [Authorize(Roles = TestRoles.ADMIN)]
    public IAsyncEnumerable<string> StreamImplementationProtected() => ReadStream("secret");
    public IAsyncEnumerable<string> StreamDefault() => ReadStream("default");
    [AllowAnonymous]
    public IAsyncEnumerable<string> StreamPublicOverride() => ReadStream("public");
    public Task<(int Tasks, int Streams)> Counts() => Task.FromResult((tasks, streams));

    private Task<string> ReadTask(string value)
    {
        tasks++;
        return Task.FromResult(value);
    }

    private async IAsyncEnumerable<string> ReadStream(string value)
    {
        streams++;
        yield return value;
        await Task.Yield();
        yield return value;
    }
}

public interface IUnannotatedAuthorizationGrain : IGrainWithGuidKey
{
    Task<string> ReadTask();
    IAsyncEnumerable<string> ReadStream();
}

public sealed class UnannotatedAuthorizationGrain : Grain, IUnannotatedAuthorizationGrain
{
    public Task<string> ReadTask() => Task.FromResult("unannotated");
    public async IAsyncEnumerable<string> ReadStream()
    {
        yield return "unannotated";
        await Task.Yield();
    }
}
