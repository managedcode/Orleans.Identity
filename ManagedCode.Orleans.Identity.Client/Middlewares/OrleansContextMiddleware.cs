using System.Threading.Tasks;
using ManagedCode.Orleans.Identity.Core.Extensions;
using Microsoft.AspNetCore.Http;

namespace ManagedCode.Orleans.Identity.Client.Middlewares;

public class OrleansContextMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context)
    {
        context.User.SetOrleansContext();
        await next(context);
    }
}