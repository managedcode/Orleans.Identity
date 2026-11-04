using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Security.Claims;
using System.Threading.Channels;
using ManagedCode.Orleans.Identity.Client.Extensions;
using ManagedCode.Orleans.Identity.Client.Filters;
using ManagedCode.Orleans.Identity.Core.Constants;
using ManagedCode.Orleans.Identity.Tests.TestApp;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Orleans.Runtime;
using Shouldly;
using Xunit;

namespace ManagedCode.Orleans.Identity.Tests;

public class ClientFilterTests
{
    private const string AuthenticationType = "Test";
    private const string CurrentUserName = "current-user";
    private const string PreviousUserName = "previous-user";

    [Fact]
    public async Task OrleansAuthorizationActionFilter_RestoresPreviousRequestContext()
    {
        try
        {
            var previousUser = CreatePrincipal(PreviousUserName);
            var currentUser = CreatePrincipal(CurrentUserName);
            var context = CreateActionExecutingContext(currentUser);
            var filter = new OrleansAuthorizationActionFilter();

            RequestContext.Set(OrleansIdentityConstants.USER_CLAIMS, previousUser);

            await filter.OnActionExecutionAsync(
                context,
                () =>
                {
                    RequestContext.Get(OrleansIdentityConstants.USER_CLAIMS).ShouldBeSameAs(currentUser);
                    return Task.FromResult(CreateActionExecutedContext(context));
                }
            );

            RequestContext.Get(OrleansIdentityConstants.USER_CLAIMS).ShouldBeSameAs(previousUser);
        }
        finally
        {
            RequestContext.Clear();
        }
    }

    [Fact]
    public async Task OrleansAuthorizationActionFilter_ClearsRequestContextWhenNoPreviousValue()
    {
        try
        {
            var currentUser = CreatePrincipal(CurrentUserName);
            var context = CreateActionExecutingContext(currentUser);
            var filter = new OrleansAuthorizationActionFilter();

            RequestContext.Remove(OrleansIdentityConstants.USER_CLAIMS);

            await filter.OnActionExecutionAsync(
                context,
                () =>
                {
                    RequestContext.Get(OrleansIdentityConstants.USER_CLAIMS).ShouldBeSameAs(currentUser);
                    return Task.FromResult(CreateActionExecutedContext(context));
                }
            );

            RequestContext.Get(OrleansIdentityConstants.USER_CLAIMS).ShouldBeNull();
        }
        finally
        {
            RequestContext.Clear();
        }
    }

    [Fact]
    public void AddOrleansIdentity_RegistersClientFiltersOnce()
    {
        using var services = new ServiceCollection()
            .AddOrleansIdentity()
            .AddOrleansIdentity()
            .BuildServiceProvider();

        var mvc = services.GetRequiredService<IOptions<MvcOptions>>().Value;
        mvc.Filters.OfType<TypeFilterAttribute>().Count(filter =>
            filter.ImplementationType == typeof(OrleansAuthorizationActionFilter)).ShouldBe(1);
        services.GetServices<IConfigureOptions<HubOptions>>().Count().ShouldBe(1);
    }

    [Theory]
    [InlineData(false, false)]
    [InlineData(false, true)]
    [InlineData(true, false)]
    [InlineData(true, true)]
    public async Task SignalRLifetimeCallback_ScopesItsOwnPrincipal(bool disconnecting, bool authenticated)
    {
        var previous = CreatePrincipal(PreviousUserName);
        var current = authenticated ? CreatePrincipal(CurrentUserName) : null;
        using var hub = new LifecycleHub();
        using var services = new ServiceCollection().BuildServiceProvider();
        var context = new HubLifetimeContext(new LifecycleCallerContext(current), services, hub);
        var filter = new SignalRAuthorizationFilter();

        RequestContext.Set(OrleansIdentityConstants.USER_CLAIMS, previous);
        try
        {
            if (disconnecting)
            {
                await filter.OnDisconnectedAsync(context, null, (_, _) => AssertPrincipalAsync());
            }
            else
            {
                await filter.OnConnectedAsync(context, _ => AssertPrincipalAsync());
            }

            RequestContext.Get(OrleansIdentityConstants.USER_CLAIMS).ShouldBeSameAs(previous);
        }
        finally
        {
            RequestContext.Clear();
        }

        async Task AssertPrincipalAsync()
        {
            await Task.Yield();
            RequestContext.Get(OrleansIdentityConstants.USER_CLAIMS).ShouldBeSameAs(current);
        }
    }

    [Fact]
    public async Task SignalRChannel_PreservesReaderCapabilitiesAndCancellation()
    {
        var channel = Channel.CreateUnbounded<string>();
        var user = CreatePrincipal(CurrentUserName);
        using var hub = new TestAuthorizeHub(null!);
        using var services = new ServiceCollection().BuildServiceProvider();
        var method = typeof(TestAuthorizeHub).GetMethod(nameof(TestAuthorizeHub.ReadChannel))!;
        var context = new HubInvocationContext(new LifecycleCallerContext(user), services, hub, method, []);
        var result = await new SignalRAuthorizationFilter().InvokeMethodAsync(context,
            _ => ValueTask.FromResult<object?>(channel.Reader));
        var reader = Assert.IsAssignableFrom<ChannelReader<string>>(result);

        reader.Completion.ShouldBeSameAs(channel.Reader.Completion);
        reader.CanCount.ShouldBe(channel.Reader.CanCount);
        reader.CanPeek.ShouldBe(channel.Reader.CanPeek);
        using var cancellation = new CancellationTokenSource();
        var pending = reader.ReadAsync(cancellation.Token).AsTask();
        await cancellation.CancelAsync();
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => pending);

        channel.Writer.TryWrite("first").ShouldBeTrue();
        channel.Writer.TryWrite("second").ShouldBeTrue();
        channel.Writer.Complete();

        reader.Count.ShouldBe(2);
        reader.TryPeek(out var first).ShouldBeTrue();
        first.ShouldBe("first");
        (await reader.ReadAsync()).ShouldBe("first");
        var remaining = new List<string>();
        await foreach (var item in reader.ReadAllAsync()) remaining.Add(item);
        remaining.ShouldBe(["second"]);
        await reader.Completion;
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task SignalRChannel_ReaderPropertiesUseTheInvocationPrincipal(bool authenticated)
    {
        var user = authenticated ? CreatePrincipal(CurrentUserName) : null;
        var previous = CreatePrincipal(PreviousUserName);
        var source = new PrincipalCheckingChannelReader(user);
        using var hub = new TestAuthorizeHub(null!);
        using var services = new ServiceCollection().BuildServiceProvider();
        var method = typeof(TestAuthorizeHub).GetMethod(nameof(TestAuthorizeHub.ReadChannel))!;
        var context = new HubInvocationContext(new LifecycleCallerContext(user), services, hub, method, []);
        RequestContext.Set(OrleansIdentityConstants.USER_CLAIMS, previous);
        try
        {
            var result = await new SignalRAuthorizationFilter().InvokeMethodAsync(context, _ => ValueTask.FromResult<object?>(source));
            var reader = Assert.IsAssignableFrom<ChannelReader<string>>(result);
            reader.Completion.ShouldBeSameAs(Task.CompletedTask);
            reader.CanCount.ShouldBeTrue();
            reader.CanPeek.ShouldBeTrue();
            reader.Count.ShouldBe(0);
            RequestContext.Get(OrleansIdentityConstants.USER_CLAIMS).ShouldBeSameAs(previous);
        }
        finally
        {
            RequestContext.Clear();
        }
    }

    private sealed class PrincipalCheckingChannelReader(ClaimsPrincipal? user) : ChannelReader<string>
    {
        public override Task Completion => CheckPrincipal(Task.CompletedTask);
        public override bool CanCount => CheckPrincipal(true);
        public override bool CanPeek => CheckPrincipal(true);
        public override int Count => CheckPrincipal(0);
        public override bool TryRead(out string item)
        {
            item = string.Empty;
            return false;
        }

        public override ValueTask<bool> WaitToReadAsync(CancellationToken cancellationToken = default) => ValueTask.FromResult(false);

        private T CheckPrincipal<T>(T value)
        {
            RequestContext.Get(OrleansIdentityConstants.USER_CLAIMS).ShouldBeSameAs(user);
            return value;
        }
    }

    [Fact]
    public void SignalRWrapperCache_DoesNotKeepCollectibleReturnTypesAlive()
    {
        var payloadType = CacheCollectibleChannel();

        for (var attempt = 0; attempt < 10 && payloadType.IsAlive; attempt++)
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
        }

        payloadType.IsAlive.ShouldBeFalse();
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static WeakReference CacheCollectibleChannel()
    {
        var assembly = AssemblyBuilder.DefineDynamicAssembly(new AssemblyName("CollectibleChannelHub"), AssemblyBuilderAccess.RunAndCollect);
        var module = assembly.DefineDynamicModule("Hub");
        var payloadType = module.DefineType("Payload", TypeAttributes.Public).CreateType()!;
        var hubType = typeof(CollectibleChannelHub<>).MakeGenericType(payloadType);
        using var hub = (Hub)Activator.CreateInstance(hubType, nonPublic: true)!;
        using var services = new ServiceCollection().BuildServiceProvider();
        var method = hubType.GetMethod(nameof(CollectibleChannelHub<object>.Read))!;
        var context = new HubInvocationContext(new LifecycleCallerContext(null), services, hub, method, []);
        var result = new SignalRAuthorizationFilter().InvokeMethodAsync(context,
            _ => ValueTask.FromResult(method.Invoke(hub, null))).AsTask().GetAwaiter().GetResult();
        method.ReturnType.IsInstanceOfType(result).ShouldBeTrue();
        return new WeakReference(payloadType);
    }

    private sealed class CollectibleChannelHub<T> : Hub
    {
        private readonly Channel<T> channel = Channel.CreateUnbounded<T>();

        public ChannelReader<T> Read() => channel.Reader;
    }

    private sealed class LifecycleHub : Hub;

    private sealed class LifecycleCallerContext(ClaimsPrincipal? user) : HubCallerContext
    {
        public override string ConnectionId => "lifecycle-test";
        public override string? UserIdentifier => user?.Identity?.Name;
        public override ClaimsPrincipal? User => user;
        public override IDictionary<object, object?> Items { get; } = new Dictionary<object, object?>();
        public override IFeatureCollection Features { get; } = new FeatureCollection();
        public override CancellationToken ConnectionAborted => CancellationToken.None;
        public override void Abort() => throw new NotSupportedException();
    }

    private static ClaimsPrincipal CreatePrincipal(string userName)
    {
        return new ClaimsPrincipal(new ClaimsIdentity([new Claim(ClaimTypes.Name, userName)], AuthenticationType));
    }

    private static ActionExecutingContext CreateActionExecutingContext(ClaimsPrincipal user)
    {
        var httpContext = new DefaultHttpContext { User = user };
        var actionContext = new ActionContext(httpContext, new RouteData(), new ActionDescriptor());

        return new ActionExecutingContext(actionContext, [], new Dictionary<string, object?>(), new object());
    }

    private static ActionExecutedContext CreateActionExecutedContext(ActionExecutingContext context)
    {
        return new ActionExecutedContext(context, [], context.Controller);
    }
}
