using ManagedCode.Repository.Core;

namespace ManagedCode.Identity.Core.Common
{
    public class UserRoleRelation<TId, TUserKey, TRoleKey> : IItem<TId>
    {
        public TUserKey UserKey { get; set; }
        public TRoleKey RoleKey { get; set; }
        public TId Id { get; set; }
    }
}