using System.Runtime.CompilerServices;
using ManagedCode.Orleans.Identity.Tests.Constants;
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
}

public sealed class StreamingAuthorizationGrain : Grain, IStreamingAuthorizationGrain
{
    public const string ResourcePolicy = "stream-original-resource";
    private int started;
    private int disposed;

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
