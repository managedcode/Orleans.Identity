using System;
using ManagedCode.Repository.Core;
using Microsoft.AspNetCore.Identity;

namespace ManagedCode.Orleans.Identity.old.Common
{
    public interface IIdentityUserRepository<in TKey, TUser> : IRepository<TKey, TUser>
        where TKey : IEquatable<TKey>
        where TUser : IdentityUser<TKey>, IItem<TKey>
    {
    }
}