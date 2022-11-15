using ManagedCode.Orleans.Identity.old.Common;
using ManagedCode.Repository.Core;

namespace ManagedCode.Orleans.Identity.old
{
    public class InMemoryIdentityRoleRepository : InMemoryRepository<string, InMemoryIdentityRole>,
        IIdentityRoleRepository<string, InMemoryIdentityRole>
    {
    
    }
}