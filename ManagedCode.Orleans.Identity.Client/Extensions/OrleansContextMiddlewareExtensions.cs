using ManagedCode.Orleans.Identity.Client.Middlewares;
using Microsoft.AspNetCore.Builder;

namespace ManagedCode.Orleans.Identity.Client.Extensions;

public static class OrleansContextMiddlewareExtensions
{
    /// <summary>
    /// Use middleware to set claims and session id for request to cluster
    /// </summary>
    /// <param name="builder"></param>
    /// <returns></returns>
    public static IApplicationBuilder UseOrleansIdentity(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<OrleansContextMiddleware>()
            .UseMiddleware<OrleansContextMiddleware>();
    }

    /// <summary>
    /// Use middleware to set claims and session id for request to cluster, this method includes <c>UseAuthentication</c> and <c>UseAuthorization</c> middlewares
    /// </summary>
    /// <param name="builder"></param>
    /// <returns></returns>
    public static IApplicationBuilder UseAuthenticationAndOrleansIdentity(this IApplicationBuilder builder)
    {
        builder.UseAuthentication();
        builder.UseAuthorization();
        return builder.UseOrleansIdentity();
    }
}