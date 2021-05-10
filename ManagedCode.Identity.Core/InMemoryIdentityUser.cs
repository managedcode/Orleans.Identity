using System.Collections.Generic;
using ManagedCode.Repository.Core;
using Microsoft.AspNetCore.Identity;

namespace ManagedCode.Identity.Core
{
    public class InMemoryIdentityUser : IdentityUser<string>, IItem<string>
    {
        public InMemoryIdentityUser()
        {
            Roles = new List<string>();
            Logins = new List<IdentityUserLogin<string>>();
            Claims = new List<IdentityUserClaim<string>>();
            Tokens = new List<IdentityUserToken<string>>();
        }

        public List<string> Roles { get; set; }
        public List<IdentityUserLogin<string>> Logins { get; }
        public List<IdentityUserClaim<string>> Claims { get; }
        public List<IdentityUserToken<string>> Tokens { get; }
    }
}