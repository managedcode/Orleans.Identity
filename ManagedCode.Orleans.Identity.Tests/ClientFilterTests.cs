using System.Security.Claims;
using ManagedCode.Orleans.Identity.Client.Filters;
using ManagedCode.Orleans.Identity.Core.Constants;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
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
