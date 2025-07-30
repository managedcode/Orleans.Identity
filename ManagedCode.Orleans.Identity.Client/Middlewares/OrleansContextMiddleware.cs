using System.Security.Claims;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Orleans.Runtime;

namespace ManagedCode.Orleans.Identity.Client.Middlewares;

public class OrleansContextMiddleware : IMiddleware
{
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        if (context.User.Identity?.IsAuthenticated == true)
        {
            // Store user claims in Orleans RequestContext as serializable dictionary
            // Group claims by type to handle multiple values (like roles)
            var claims = context.User.Claims
                .GroupBy(c => c.Type)
                .ToDictionary(g => g.Key, g => string.Join(",", g.Select(c => c.Value)));
            RequestContext.Set("UserClaims", claims);
        }

        await next(context);
    }
}