using System;
using ManagedCode.Identity.Core.Common;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace ManagedCode.Identity.Core.Extensions
{
    public static class InMemoryIdentityExtensions
    {
        public static IdentityBuilder RegisterInMemoryIdentityStore<TUser, TRole>(this IdentityBuilder builder)
            where TRole : InMemoryIdentityRole
            where TUser : InMemoryIdentityUser
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
            builder.Services.AddSingleton<InMemoryIdentityStore>();

            return builder;
        }

        public static IdentityBuilder AddIdentityWithInMemoryStore(this IServiceCollection services)
        {
            return services.AddIdentityWithInMemoryStore<InMemoryIdentityUser, InMemoryIdentityRole>();
        }

        public static IdentityBuilder AddIdentityWithInMemoryStore<TUser, TRole>(this IServiceCollection services)
            where TUser : InMemoryIdentityUser
            where TRole : InMemoryIdentityRole
        {
            return services.AddIdentity<TUser, TRole>()
                .RegisterInMemoryIdentityStore<TUser, TRole>();
        }
    }
}