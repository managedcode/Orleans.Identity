using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Orleans.Runtime;
using ManagedCode.Orleans.Identity.Core.Constants;

namespace ManagedCode.Orleans.Identity.Client.Middlewares;

public class OrleansContextMiddleware : IMiddleware
{
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        if (context.User.Identity?.IsAuthenticated == true)
        {
            RequestContext.Set(OrleansIdentityConstants.USER_CLAIMS, context.User);
        }

        await next(context);
    }
}