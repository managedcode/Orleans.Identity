using ManagedCode.Repository.Core;

namespace ManagedCode.Identity.Core.Common
{
    public interface IUserRoleRelationRepository<TId, TUserKey, TRoleKey> : IRepository<TId, UserRoleRelation<TId, TUserKey, TRoleKey>>
    {
    }
}