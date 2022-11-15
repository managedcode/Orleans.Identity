using ManagedCode.Repository.Core;

namespace ManagedCode.Orleans.Identity.old.Common
{
    public interface IUserRoleRelationRepository<TId, TUserKey, TRoleKey> : IRepository<TId, UserRoleRelation<TId, TUserKey, TRoleKey>>
    {
    }
}