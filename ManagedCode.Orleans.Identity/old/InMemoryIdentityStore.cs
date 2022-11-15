using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using ManagedCode.Orleans.Identity.old.Common;
using ManagedCode.Orleans.Identity.old.Extensions;
using Microsoft.AspNetCore.Identity;

namespace ManagedCode.Orleans.Identity.old
{
    public class InMemoryIdentityStore : BaseIdentityStore<string, InMemoryIdentityUser, InMemoryIdentityRole>
    {
        private readonly IIdentityRoleRepository<string, InMemoryIdentityRole> _roleRepository;
        private readonly IIdentityUserRepository<string, InMemoryIdentityUser> _userRepository;

        public InMemoryIdentityStore(
            IIdentityUserRepository<string, InMemoryIdentityUser> userRepository,
            IIdentityRoleRepository<string, InMemoryIdentityRole> roleRepository) 
        {
            _userRepository = userRepository;
            _roleRepository = roleRepository;
        }

        protected override async Task<IdentityResult> CreateAsyncInternal(InMemoryIdentityUser user, CancellationToken cancellationToken)
        {
            var result = await _userRepository.InsertAsync(user, cancellationToken);
            return result != null ? IdentityResult.Success : IdentityResult.Failed();
        }

        protected override async Task<IdentityResult> UpdateAsyncInternal(InMemoryIdentityUser user, CancellationToken cancellationToken)
        {
            var result = await _userRepository.UpdateAsync(user, cancellationToken);
            return result != null ? IdentityResult.Success : IdentityResult.Failed();
        }

        protected override async Task<IdentityResult> DeleteAsyncInternal(InMemoryIdentityUser user, CancellationToken cancellationToken)
        {
            var result = await _userRepository.DeleteAsync(user, cancellationToken);
            return result ? IdentityResult.Success : IdentityResult.Failed();
        }

        protected override async Task<IdentityResult> CreateAsyncInternal(InMemoryIdentityRole role, CancellationToken cancellationToken)
        {
            var result = await _roleRepository.InsertAsync(role, cancellationToken);
            return result != null ? IdentityResult.Success : IdentityResult.Failed();
        }

        protected override async Task<IdentityResult> UpdateAsyncInternal(InMemoryIdentityRole role, CancellationToken cancellationToken)
        {
            var result = await _roleRepository.UpdateAsync(role, cancellationToken);
            return result != null ? IdentityResult.Success : IdentityResult.Failed();
        }

        protected override async Task<IdentityResult> DeleteAsyncInternal(InMemoryIdentityRole role, CancellationToken cancellationToken)
        {
            var result = await _roleRepository.DeleteAsync(role, cancellationToken);
            return result ? IdentityResult.Success : IdentityResult.Failed();
        }

        protected override async Task<InMemoryIdentityRole> FindRoleByIdAsyncInternal(string roleId, CancellationToken cancellationToken)
        {
            return await _roleRepository.GetAsync(w => w.Id == roleId, cancellationToken);
        }

        protected override async Task<InMemoryIdentityRole> FindRoleByNameAsyncInternal(string normalizedRoleName, CancellationToken cancellationToken)
        {
            return await _roleRepository.GetAsync(w => w.NormalizedName == normalizedRoleName, cancellationToken);
        }

        protected override async Task<InMemoryIdentityUser> FindByIdAsyncInternal(string userId, CancellationToken cancellationToken)
        {
            return await _userRepository.GetAsync(w => w.Id == userId, cancellationToken);
        }

        protected override async Task<InMemoryIdentityUser> FindByNameAsyncInternal(string normalizedUserName, CancellationToken cancellationToken)
        {
            return await _userRepository.GetAsync(w => w.NormalizedUserName == normalizedUserName, cancellationToken);
        }

        protected override Task AddToRoleAsyncInternal(InMemoryIdentityUser user, string roleName, CancellationToken cancellationToken)
        {
            user.Roles.Add(roleName);
            return Task.CompletedTask;
        }

        protected override Task RemoveFromRoleAsyncInternal(InMemoryIdentityUser user, string roleName, CancellationToken cancellationToken)
        {
            user.Roles.Remove(roleName);
            return Task.CompletedTask;
        }

        protected override Task<IList<string>> GetRolesAsyncInternal(InMemoryIdentityUser user, CancellationToken cancellationToken)
        {
            return Task.FromResult((IList<string>) user.Roles);
        }

        protected override Task<bool> IsInRoleAsyncInternal(InMemoryIdentityUser user, string roleName, CancellationToken cancellationToken)
        {
            return Task.FromResult(user.Roles.Contains(roleName));
        }

        protected override async Task<IList<InMemoryIdentityUser>> GetUsersInRoleAsyncInternal(string roleName, CancellationToken cancellationToken)
        {
            IList<InMemoryIdentityUser> results = new List<InMemoryIdentityUser>();
            await foreach (var item in _userRepository.FindAsync(w => w.Roles.Contains(roleName), token: cancellationToken))
            {
                results.Add(item);
            }

            return results;
        }

        protected override Task<IList<Claim>> GetClaimsAsyncInternal(InMemoryIdentityUser user, CancellationToken cancellationToken)
        {
            return Task.FromResult((IList<Claim>) user.Claims.Select(s => s.ToClaim()).ToList());
        }

        protected override Task AddClaimsAsyncInternal(InMemoryIdentityUser user, IEnumerable<Claim> claims, CancellationToken cancellationToken)
        {
            foreach (var item in claims)
            {
                var identityUserClaim = new IdentityUserClaim<string>();
                identityUserClaim.InitializeFromClaim(item);
                user.Claims.Add(identityUserClaim);
            }

            return Task.CompletedTask;
        }

        protected override Task ReplaceClaimAsyncInternal(InMemoryIdentityUser user, Claim claim, Claim newClaim, CancellationToken cancellationToken)
        {
            var old = user.Claims.FirstOrDefault(w => w.ClaimType == claim.Type && w.ClaimValue == claim.Value);
            if (old != null)
            {
                var c = new IdentityUserClaim<string>();
                c.InitializeFromClaim(newClaim);
                user.Claims[user.Claims.IndexOf(old)] = c;
            }

            return Task.CompletedTask;
        }

        protected override Task RemoveClaimsAsyncInternal(InMemoryIdentityUser user, IEnumerable<Claim> claims, CancellationToken cancellationToken)
        {
            foreach (var claim in claims)
            {
                var old = user.Claims.FirstOrDefault(w => w.ClaimType == claim.Type && w.ClaimValue == claim.Value);
                if (old != null)
                {
                    user.Claims.Remove(old);
                }
            }

            return Task.CompletedTask;
        }

        protected override async Task<IList<InMemoryIdentityUser>> GetUsersForClaimAsyncInternal(Claim claim, CancellationToken cancellationToken)
        {
            IList<InMemoryIdentityUser> users = new List<InMemoryIdentityUser>();
            await foreach (var item in _userRepository.FindAsync(w => w.Claims.Any(a => a.ClaimType == claim.Type && a.ClaimValue == claim.Value),
                token: cancellationToken))
            {
                users.Add(item);
            }

            return users;
        }

        protected override async Task<InMemoryIdentityUser> FindByEmailAsyncInternal(string normalizedEmail, CancellationToken cancellationToken)
        {
            return await _userRepository.GetAsync(w => w.NormalizedEmail == normalizedEmail, cancellationToken);
        }

        protected override IQueryable<InMemoryIdentityUser> UsersInternal()
        {
            throw new NotImplementedException();
        }

        protected override Task AddLoginAsyncInternal(InMemoryIdentityUser user, UserLoginInfo login, CancellationToken cancellationToken)
        {
            user.Logins.Add(login.ToIdentityUserLogin(user));
            return Task.CompletedTask;
        }

        protected override Task RemoveLoginAsyncInternal(InMemoryIdentityUser user,
            string loginProvider,
            string providerKey,
            CancellationToken cancellationToken)
        {
            var old = user.Logins.FirstOrDefault(f => f.LoginProvider == loginProvider && f.ProviderKey == providerKey);
            if (old != null)
            {
                user.Logins.Remove(old);
            }

            return Task.CompletedTask;
        }

        protected override Task<IList<UserLoginInfo>> GetLoginsAsyncInternal(InMemoryIdentityUser user, CancellationToken cancellationToken)
        {
            return Task.FromResult((IList<UserLoginInfo>) user.Logins.Select(s => s.ToUserLoginInfo()).ToList());
        }

        protected override async Task<InMemoryIdentityUser> FindByLoginAsyncInternal(string loginProvider,
            string providerKey,
            CancellationToken cancellationToken)
        {
            return await _userRepository.GetAsync(w => w.Logins.Any(a => a.LoginProvider == loginProvider && a.ProviderKey == providerKey), cancellationToken);
        }

        protected override Task SetTokenAsyncInternal(InMemoryIdentityUser user,
            string loginProvider,
            string name,
            string value,
            CancellationToken cancellationToken)
        {
            user.Tokens.Add(new IdentityUserToken<string>
            {
                LoginProvider = loginProvider,
                Name = name,
                UserId = user.Id,
                Value = value
            });
            return Task.CompletedTask;
        }

        protected override Task RemoveTokenAsyncInternal(InMemoryIdentityUser user, string loginProvider, string name, CancellationToken cancellationToken)
        {
            var old = user.Tokens.FirstOrDefault(w => w.LoginProvider == loginProvider && w.Name == name);
            if (old != null)
            {
                user.Tokens.Remove(old);
            }

            return Task.CompletedTask;
        }

        protected override Task<string> GetTokenAsyncInternal(InMemoryIdentityUser user, string loginProvider, string name, CancellationToken cancellationToken)
        {
            return Task.FromResult(user.Tokens.FirstOrDefault(f => f.LoginProvider == loginProvider && f.Name == name)?.Value);
        }

        protected override IQueryable<InMemoryIdentityRole> RolesInternal()
        {
            throw new NotImplementedException();
        }

        protected override async Task<IList<Claim>> GetClaimsAsyncInternal(InMemoryIdentityRole role, CancellationToken cancellationToken)
        {
            var claims = new HashSet<Claim>();
            await foreach (var user in _userRepository.GetAllAsync(token: cancellationToken))
            {
                foreach (var userClaim in user.Claims)
                {
                    var claim = userClaim.ToClaim();
                    if (!claims.TryGetValue(claim, out _))
                    {
                        claims.Add(claim);
                    }
                }
            }

            return claims.ToList();
        }

        protected override Task AddClaimAsyncInternal(InMemoryIdentityRole role, Claim claim, CancellationToken cancellationToken)
        {
            role.Claims.Add(claim);
            return Task.CompletedTask;
        }

        protected override Task RemoveClaimAsyncInternal(InMemoryIdentityRole role, Claim claim, CancellationToken cancellationToken)
        {
            role.Claims.Remove(claim);
            return Task.CompletedTask;
        }
    }
}