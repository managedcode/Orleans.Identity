using ManagedCode.Identity.Core.Common;
using ManagedCode.Repository.Core;
using Microsoft.Extensions.Logging;

namespace ManagedCode.Identity.Core
{
    public class InMemoryIdentityUserRepository : InMemoryRepository<string, InMemoryIdentityUser>,
        IIdentityUserRepository<string, InMemoryIdentityUser>
    {
    }
}