using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Filters;
using Orleans.Runtime;
using ManagedCode.Orleans.Identity.Core.Constants;

namespace ManagedCode.Orleans.Identity.Client.Filters;

public sealed class OrleansAuthorizationActionFilter : IAsyncActionFilter
{
    public Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        RequestContext.Set(OrleansIdentityConstants.USER_CLAIMS, context.HttpContext.User);
        return next();
    }
}