using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Filters;

namespace ManagedCode.Orleans.Identity.Client.Filters;

public sealed class OrleansAuthorizationActionFilter : IAsyncResourceFilter, IAsyncActionFilter
{
    public async Task OnResourceExecutionAsync(ResourceExecutingContext context, ResourceExecutionDelegate next)
    {
        using var requestContextScope = new OrleansRequestContextScope(context.HttpContext.User);
        await next();
    }

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        using var requestContextScope = new OrleansRequestContextScope(context.HttpContext.User);
        await next();
    }
}
