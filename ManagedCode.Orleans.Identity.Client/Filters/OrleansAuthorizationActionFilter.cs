using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Filters;

namespace ManagedCode.Orleans.Identity.Client.Filters;

public sealed class OrleansAuthorizationActionFilter : IAsyncActionFilter
{
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        using var requestContextScope = new OrleansRequestContextScope(context.HttpContext.User);
        await next();
    }
}
