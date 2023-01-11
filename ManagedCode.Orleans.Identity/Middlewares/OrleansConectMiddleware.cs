using System.Globalization;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace ManagedCode.Orleans.Identity.Middlewares;

public class OrleansContextMiddleware
{
    private readonly RequestDelegate _next;

    public OrleansContextMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        context.User.SetOrleansContext();
        await _next(context);
    }
}

public static class OrleansContextMiddlewareExtensions
{
    // public static IApplicationBuilder UseOrleansContextMiddleware(this IApplicationBuilder builder)
    // {
    //     return builder.UseMiddleware<OrleansContextMiddleware>();
    // }
    
    public static IApplicationBuilder UseOrleansAuthorization(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<OrleansContextMiddleware>();
    }
}