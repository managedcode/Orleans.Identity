using System.Collections.Generic;
using System.Security.Claims;
using ManagedCode.Repository.Core;
using Microsoft.AspNetCore.Identity;

namespace ManagedCode.Identity.Core
{
    public class InMemoryIdentityRole : IdentityRole<string>, IItem<string>
    {
        public InMemoryIdentityRole()
        {
            Claims = new List<Claim>();
        }

        public List<Claim> Claims { get; }
    }
}