using ManagedCode.Orleans.Identity.old.Common;
using ManagedCode.Repository.Core;

namespace ManagedCode.Orleans.Identity.old
{
    public class InMemoryIdentityUserRepository : InMemoryRepository<string, InMemoryIdentityUser>,
        IIdentityUserRepository<string, InMemoryIdentityUser>
    {
    }
}