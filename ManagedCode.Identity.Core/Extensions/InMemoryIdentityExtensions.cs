using System;
using ManagedCode.Identity.Core.Common;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace ManagedCode.Identity.Core.Extensions
{
    public static class InMemoryIdentityExtensions
    {
        public static IdentityBuilder RegisterInMemoryIdentityStore<TUser, TRole>(this IdentityBuilder builder)
            where TUser : InMemoryIdentityUser
            where TRole : InMemoryIdentityRole
        {
            if (typeof(TUser) != builder.UserType)
            {
                var message = "User type passed to IdentityStore must match user type passed to AddIdentity. " +
                              $"You passed {builder.UserType} to AddIdentity and {typeof(TUser)} to IdentityStore, " +
                              "these do not match.";
                throw new ArgumentException(message);
            }

            if (typeof(TRole) != builder.RoleType)
            {
                var message = "Role type passed to IdentityStore must match role type passed to AddIdentity. " +
                              $"You passed {builder.RoleType} to AddIdentity and {typeof(TRole)} to IdentityStore, " +
                              "these do not match.";
                throw new ArgumentException(message);
            }

            builder.Services.AddSingleton<IIdentityUserRepository<string, InMemoryIdentityUser>, InMemoryIdentityUserRepository>();
            builder.Services.AddSingleton<IIdentityRoleRepository<string, InMemoryIdentityRole>, InMemoryIdentityRoleRepository>();
            
            builder.Services.AddSingleton<IUserLoginStore<InMemoryIdentityUser>, InMemoryIdentityStore>();
            builder.Services.AddSingleton<IUserRoleStore<InMemoryIdentityUser>, InMemoryIdentityStore>();
            builder.Services.AddSingleton<IUserClaimStore<InMemoryIdentityUser>, InMemoryIdentityStore>();
            builder.Services.AddSingleton<IUserPasswordStore<InMemoryIdentityUser>, InMemoryIdentityStore>();
            builder.Services.AddSingleton<IUserSecurityStampStore<InMemoryIdentityUser>, InMemoryIdentityStore>();
            builder.Services.AddSingleton<IUserEmailStore<InMemoryIdentityUser>, InMemoryIdentityStore>();
            builder.Services.AddSingleton<IUserLockoutStore<InMemoryIdentityUser>, InMemoryIdentityStore>();
            builder.Services.AddSingleton<IUserPhoneNumberStore<InMemoryIdentityUser>, InMemoryIdentityStore>();
            builder.Services.AddSingleton<IQueryableUserStore<InMemoryIdentityUser>, InMemoryIdentityStore>();
            builder.Services.AddSingleton<IUserTwoFactorStore<InMemoryIdentityUser>, InMemoryIdentityStore>();
            builder.Services.AddSingleton<IQueryableRoleStore<InMemoryIdentityRole>, InMemoryIdentityStore>();
            builder.Services.AddSingleton<IRoleClaimStore<InMemoryIdentityRole>, InMemoryIdentityStore>();
            builder.Services.AddSingleton<IUserAuthenticationTokenStore<InMemoryIdentityUser>, InMemoryIdentityStore>();

            return builder;
        }

        public static IdentityBuilder AddIdentityWithInMemoryStore(this IServiceCollection services)
        {
            return services.AddIdentityWithInMemoryStore<InMemoryIdentityUser, InMemoryIdentityRole>();
        }

        public static IdentityBuilder AddIdentityWithInMemoryStore(this IServiceCollection services, Action<IdentityOptions> options)
        {
            return services.AddIdentityWithInMemoryStore<InMemoryIdentityUser, InMemoryIdentityRole>(options);
        }

        public static IdentityBuilder AddIdentityWithInMemoryStore<TUser, TRole>(this IServiceCollection services)
            where TUser : InMemoryIdentityUser
            where TRole : InMemoryIdentityRole
        {
            return services.AddIdentity<TUser, TRole>()
                .RegisterInMemoryIdentityStore<TUser, TRole>();
        }

        public static IdentityBuilder AddIdentityWithInMemoryStore<TUser, TRole>(this IServiceCollection services, Action<IdentityOptions> options)
            where TUser : InMemoryIdentityUser
            where TRole : InMemoryIdentityRole
        {
            return services.AddIdentity<TUser, TRole>(options)
                .RegisterInMemoryIdentityStore<TUser, TRole>();
        }
    }
}