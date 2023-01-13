using System.Threading.Tasks;
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