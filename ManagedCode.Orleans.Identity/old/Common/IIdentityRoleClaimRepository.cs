using System;
using ManagedCode.Repository.Core;

namespace ManagedCode.Orleans.Identity.old.Common
{
    public interface IIdentityRoleClaimRepository<in TKey, TRoleKey, TRoleClaim> : IRepository<TKey, TRoleClaim>
        where TKey : IEquatable<TKey>
        where TRoleClaim : IdentityRoleClaim<TKey, TRoleKey>, IItem<TKey>, new()
        where TRoleKey : IEquatable<TRoleKey>

    {
    }
}