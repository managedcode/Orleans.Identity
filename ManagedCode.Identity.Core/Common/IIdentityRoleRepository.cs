using System;
using ManagedCode.Repository.Core;
using Microsoft.AspNetCore.Identity;

namespace ManagedCode.Identity.Core.Common
{
    public interface IIdentityRoleRepository<in TKey, TRole> : IRepository<TKey, TRole>
        where TKey : IEquatable<TKey>
        where TRole : IdentityRole<TKey>, IItem<TKey>

    {
    }
}