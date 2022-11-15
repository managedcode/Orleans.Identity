using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;

namespace ManagedCode.Orleans.Identity.Middlewares;

public static class AuthenticationHandlerExtensions
{
    public static void AddAuthenticationHandler(this IServiceCollection services)
    {
        services.AddAuthentication(options => options.DefaultScheme = IdentityConstants.AUTHENTICATION_TYPE)
            .AddScheme<AuthenticationSchemeOptions, OrleansIdentityAuthenticationHandler>(IdentityConstants.AUTHENTICATION_TYPE, op => { });
       
        services.AddAuthorizationCore(options =>
        {
            var defaultAuthorizationPolicyBuilder = new AuthorizationPolicyBuilder(IdentityConstants.AUTHENTICATION_TYPE);
            defaultAuthorizationPolicyBuilder = defaultAuthorizationPolicyBuilder.RequireAuthenticatedUser();
            options.DefaultPolicy = defaultAuthorizationPolicyBuilder.Build();
        });
    }
}