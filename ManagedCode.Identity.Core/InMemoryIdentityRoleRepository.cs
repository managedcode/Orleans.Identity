using ManagedCode.Identity.Core.Common;
using ManagedCode.Repository.Core;
using Microsoft.Extensions.Logging;

namespace ManagedCode.Identity.Core
{
    public class InMemoryIdentityRoleRepository : InMemoryRepository<string, InMemoryIdentityRole>,
        IIdentityRoleRepository<string, InMemoryIdentityRole>
    {
        public InMemoryIdentityRoleRepository(ILogger logger) : base(logger)
        {
        }
    }
}