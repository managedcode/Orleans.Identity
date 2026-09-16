using System.Security.Claims;
using ManagedCode.Orleans.Identity.Core.Constants;
using ManagedCode.Orleans.Identity.Tests.Cluster;
using ManagedCode.Orleans.Identity.Tests.Cluster.Grains;
using ManagedCode.Orleans.Identity.Tests.Constants;
using Orleans.Runtime;
using Shouldly;
using Xunit;

namespace ManagedCode.Orleans.Identity.Tests;

[Collection(nameof(TestClusterApplication))]
public sealed class StreamingAuthorizationTests(TestClusterApplication app)
{
    [Theory]
    [InlineData(false, false)]
    [InlineData(false, true)]
    [InlineData(true, false)]
    [InlineData(true, true)]
    public async Task ProtectedStream_RejectsUnauthorizedCallerBeforeBody(bool implementation, bool authenticated)
    {
        SetUser(authenticated ? TestRoles.USER : null);
        var grain = app.Cluster.Client.GetGrain<IStreamingAuthorizationGrain>(Guid.NewGuid());
        try
        {
            await Should.ThrowAsync<UnauthorizedAccessException>(async () =>
            {
                await foreach (var _ in implementation ? grain.ImplementationProtected("secret") : grain.InterfaceProtected("secret")) { }
            });
            (await grain.Counts()).Started.ShouldBe(0);
        }
        finally { RequestContext.Clear(); }
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task ProtectedStream_AuthorizedCallerReadsAndDisposes(bool implementation)
    {
        SetUser(TestRoles.ADMIN);
        var grain = app.Cluster.Client.GetGrain<IStreamingAuthorizationGrain>(Guid.NewGuid());
        try
        {
            var values = new List<string>();
            var stream = implementation ? grain.ImplementationProtected("item") : grain.InterfaceProtected("item");
            ((IAsyncEnumerableRequest<string>)stream).MaxBatchSize = 1;
            await foreach (var item in stream) values.Add(item);
            values.ShouldBe(["item0", "item1", "item2"]);
            (await grain.Counts()).ShouldBe((1, 1));
        }
        finally { RequestContext.Clear(); }
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Stream_OriginalResourceAndArgumentsReachPolicy(bool allowed)
    {
        SetUser(TestRoles.ADMIN);
        var grain = app.Cluster.Client.GetGrain<IStreamingAuthorizationGrain>(Guid.NewGuid());
        try
        {
            async Task Read()
            {
                var stream = grain.ResourceProtected(allowed ? "allowed" : "denied");
                ((IAsyncEnumerableRequest<string>)stream).MaxBatchSize = 1;
                await foreach (var _ in stream) { }
            }
            if (allowed) await Read();
            else await Should.ThrowAsync<UnauthorizedAccessException>(Read);
            (await grain.Counts()).Started.ShouldBe(allowed ? 1 : 0);
        }
        finally { RequestContext.Clear(); }
    }

    [Fact]
    public async Task AnonymousStream_RemainsPublic()
    {
        RequestContext.Clear();
        var grain = app.Cluster.Client.GetGrain<IStreamingAuthorizationGrain>(Guid.NewGuid());
        var values = new List<string>();
        await foreach (var value in grain.ReadPublic()) values.Add(value);
        values.ShouldBe(["public0", "public1", "public2"]);
        (await grain.Counts()).ShouldBe((1, 1));
    }

    [Fact]
    public async Task Stream_ClassPolicyIsEnforced()
    {
        var grain = app.Cluster.Client.GetGrain<IClassProtectedStreamingGrain>(Guid.NewGuid());
        try
        {
            SetUser(TestRoles.USER);
            await Should.ThrowAsync<UnauthorizedAccessException>(async () => { await foreach (var _ in grain.Read()) { } });
            SetUser(TestRoles.ADMIN);
            var values = new List<int>();
            await foreach (var value in grain.Read()) values.Add(value);
            values.ShouldBe([1]);
        }
        finally { RequestContext.Clear(); }
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Stream_RejectsIdentityChangeOnReadAndDispose(bool sameRole)
    {
        SetUser(TestRoles.ADMIN);
        var grain = app.Cluster.Client.GetGrain<IStreamingAuthorizationGrain>(Guid.NewGuid());
        var transport = grain.AsReference<IAsyncEnumerableGrainExtension>();
        var id = Guid.NewGuid();
        var request = (IAsyncEnumerableRequest<string>)grain.InterfaceProtected("secret");
        request.MaxBatchSize = 1;
        try
        {
            var first = await transport.StartEnumeration(id, request);
            first.Status.ShouldBe(EnumerationResult.Element);
            SetUser(sameRole ? TestRoles.ADMIN : TestRoles.USER, "other");
            await Should.ThrowAsync<UnauthorizedAccessException>(async () => await transport.MoveNext<string>(id));
            await Should.ThrowAsync<UnauthorizedAccessException>(async () => await transport.DisposeAsync(id));
            (await grain.Counts()).Disposed.ShouldBe(0);
            SetUser(TestRoles.ADMIN);
            (await transport.MoveNext<string>(id)).Value.ShouldBe("secret1");
            await transport.DisposeAsync(id);
            await transport.DisposeAsync(id);
            (await grain.Counts()).ShouldBe((1, 1));
        }
        finally { RequestContext.Clear(); }
    }

    [Fact]
    public async Task Stream_RejectsUnknownContinuationAndUnsupportedScheme()
    {
        SetUser(TestRoles.ADMIN);
        var grain = app.Cluster.Client.GetGrain<IStreamingAuthorizationGrain>(Guid.NewGuid());
        try
        {
            await Should.ThrowAsync<UnauthorizedAccessException>(async () =>
                await grain.AsReference<IAsyncEnumerableGrainExtension>().MoveNext<string>(Guid.NewGuid()));
            await Should.ThrowAsync<InvalidOperationException>(async () =>
            { await foreach (var _ in grain.UnsupportedScheme()) { } });
        }
        finally { RequestContext.Clear(); }
    }

    [Fact]
    public async Task Stream_GenericInterfaceMethodRetainsPolicyAndTypeArguments()
    {
        var grain = app.Cluster.Client.GetGrain<IStreamingAuthorizationGrain>(Guid.NewGuid());
        try
        {
            SetUser(TestRoles.USER);
            await Should.ThrowAsync<UnauthorizedAccessException>(async () =>
            { await foreach (var _ in grain.GenericProtected(42)) { } });
            SetUser(TestRoles.ADMIN);
            var values = new List<int>();
            await foreach (var value in grain.GenericProtected(42)) values.Add(value);
            values.ShouldBe([42]);
        }
        finally { RequestContext.Clear(); }
    }

    [Fact]
    public async Task Stream_RejectsDuplicateAndEmptyIdentityWithoutLosingOriginalEnumeration()
    {
        SetUser(TestRoles.ADMIN);
        var grain = app.Cluster.Client.GetGrain<IStreamingAuthorizationGrain>(Guid.NewGuid());
        var transport = grain.AsReference<IAsyncEnumerableGrainExtension>();
        var id = Guid.NewGuid();
        IAsyncEnumerableRequest<string> Request()
        {
            var request = (IAsyncEnumerableRequest<string>)grain.InterfaceProtected("value");
            request.MaxBatchSize = 1;
            return request;
        }
        try
        {
            await Should.ThrowAsync<UnauthorizedAccessException>(async () => await transport.StartEnumeration(Guid.Empty, Request()));
            await transport.StartEnumeration(id, Request());
            await Should.ThrowAsync<UnauthorizedAccessException>(async () => await transport.StartEnumeration(id, Request()));
            (await transport.MoveNext<string>(id)).Value.ShouldBe("value1");
            await transport.DisposeAsync(id);
            (await grain.Counts()).ShouldBe((1, 1));
        }
        finally { RequestContext.Clear(); }
    }

    [Fact]
    public async Task Stream_BoundsPendingAuthorityAndReleasesCapacityOnDisposal()
    {
        SetUser(TestRoles.ADMIN);
        var grain = app.Cluster.Client.GetGrain<IStreamingAuthorizationGrain>(Guid.NewGuid());
        var transport = grain.AsReference<IAsyncEnumerableGrainExtension>();
        var ids = new List<Guid>();
        async Task Open()
        {
            var id = Guid.NewGuid();
            var request = (IAsyncEnumerableRequest<string>)grain.InterfaceProtected("value");
            request.MaxBatchSize = 1;
            await transport.StartEnumeration(id, request);
            ids.Add(id);
        }
        try
        {
            for (var index = 0; index < 128; index++) await Open();
            await Should.ThrowAsync<UnauthorizedAccessException>(Open);
            foreach (var id in ids) await transport.DisposeAsync(id);
            ids.Clear();
            await Open();
            (await grain.Counts()).Started.ShouldBe(129);
        }
        finally
        {
            foreach (var id in ids) await transport.DisposeAsync(id);
            RequestContext.Clear();
        }
    }

    private static void SetUser(string? role, string name = "owner")
    {
        RequestContext.Clear();
        if (role is not null)
        {
            RequestContext.Set(OrleansIdentityConstants.USER_CLAIMS,
                new ClaimsPrincipal(new ClaimsIdentity([new Claim(ClaimTypes.NameIdentifier, name),
                    new Claim(ClaimTypes.Role, role)], "stream-test")));
        }
    }
}
