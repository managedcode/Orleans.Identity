using ManagedCode.Orleans.Identity.Options;
using ManagedCode.Orleans.Identity.Shared.Constants;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;

namespace ManagedCode.Orleans.Identity.Middlewares;

public static class AuthenticationHandlerExtensions
{
    public static void AddAuthenticationHandler(this IServiceCollection services, SessionOption sessionOption = null!)
    {
        sessionOption ??= new();

        services.AddAuthentication(options => options.DefaultScheme = OrleansIdentityConstants.AUTHENTICATION_TYPE)
            .AddScheme<AuthenticationSchemeOptions, OrleansIdentityAuthenticationHandler>(OrleansIdentityConstants.AUTHENTICATION_TYPE, op => { });
       
        services.AddAuthorizationCore(options =>
        {
            var defaultAuthorizationPolicyBuilder = new AuthorizationPolicyBuilder(OrleansIdentityConstants.AUTHENTICATION_TYPE);
            defaultAuthorizationPolicyBuilder = defaultAuthorizationPolicyBuilder.RequireAuthenticatedUser();
            options.DefaultPolicy = defaultAuthorizationPolicyBuilder.Build();
        });
    }
}