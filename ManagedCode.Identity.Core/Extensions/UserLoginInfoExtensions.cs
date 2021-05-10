using System;
using Microsoft.AspNetCore.Identity;

namespace ManagedCode.Identity.Core.Extensions
{
    public static class UserLoginInfoExtensions
    {
        public static IdentityUserLogin<T> ToIdentityUserLogin<T>(this UserLoginInfo userLoginInfo) where T : IEquatable<T>
        {
            return new()
            {
                LoginProvider = userLoginInfo.LoginProvider,
                ProviderKey = userLoginInfo.ProviderKey,
                ProviderDisplayName = userLoginInfo.ProviderDisplayName
            };
        }

        public static IdentityUserLogin<T> ToIdentityUserLogin<T>(this UserLoginInfo userLoginInfo, IdentityUser<T> user) where T : IEquatable<T>
        {
            return new()
            {
                LoginProvider = userLoginInfo.LoginProvider,
                ProviderKey = userLoginInfo.ProviderKey,
                ProviderDisplayName = userLoginInfo.ProviderDisplayName,
                UserId = user.Id
            };
        }

        public static UserLoginInfo ToUserLoginInfo<T>(this IdentityUserLogin<T> identityUserLogin) where T : IEquatable<T>
        {
            return new(identityUserLogin.LoginProvider, identityUserLogin.ProviderKey, identityUserLogin.ProviderDisplayName);
        }
    }
}