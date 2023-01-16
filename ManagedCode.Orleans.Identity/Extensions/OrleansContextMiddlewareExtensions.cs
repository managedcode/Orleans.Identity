using ManagedCode.Orleans.Identity.Middlewares;
using Microsoft.AspNetCore.Builder;

namespace ManagedCode.Orleans.Identity.Extensions;

public static class OrleansContextMiddlewareExtensions
{
    public static IApplicationBuilder UseOrleansIdentity(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<OrleansContextMiddleware>();
    }

    public static IApplicationBuilder UseAuthenticationAndOrleansIdentity(this IApplicationBuilder builder)
    {
        builder.UseAuthentication();
        builder.UseAuthorization();
        return builder.UseOrleansIdentity();
    }
}