using System.Security.Claims;
using ManagedCode.Orleans.Identity.Client.Extensions;
using ManagedCode.Orleans.Identity.Client.Filters;
using ManagedCode.Orleans.Identity.Core.Constants;
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
