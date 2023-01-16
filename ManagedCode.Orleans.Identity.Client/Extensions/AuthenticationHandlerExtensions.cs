using System;
using ManagedCode.Orleans.Identity.Client.Middlewares;
using ManagedCode.Orleans.Identity.Constants;
using ManagedCode.Orleans.Identity.Options;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;

namespace ManagedCode.Orleans.Identity.Client.Extensions;

public static class AuthenticationHandlerExtensions
{
    public static void AddOrleansIdentity(this IServiceCollection services, Action<SessionOption> sessionOption)
    {
        var option = new SessionOption();
        sessionOption?.Invoke(option);
        AddOrleansIdentity(services, option);
    }

    public static void AddOrleansIdentity(this IServiceCollection services, SessionOption sessionOption = null)
    {
        sessionOption ??= new SessionOption();

        services.AddAuthentication(options => options.DefaultScheme = OrleansIdentityConstants.AUTHENTICATION_TYPE)
            .AddScheme<AuthenticationSchemeOptions, OrleansIdentityAuthenticationHandler>(OrleansIdentityConstants.AUTHENTICATION_TYPE, op => { });

        services.AddAuthorization(options =>
        {
            var defaultAuthorizationPolicyBuilder = new AuthorizationPolicyBuilder(OrleansIdentityConstants.AUTHENTICATION_TYPE);
            defaultAuthorizationPolicyBuilder = defaultAuthorizationPolicyBuilder.RequireAuthenticatedUser();
            options.DefaultPolicy = defaultAuthorizationPolicyBuilder.Build();
        });
    }
}