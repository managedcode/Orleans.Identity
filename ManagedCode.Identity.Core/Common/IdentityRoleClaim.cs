using System;
using System.Security.Claims;
using ManagedCode.Repository.Core;

namespace ManagedCode.Identity.Core.Common
{
    public class IdentityRoleClaim<TKey, TRoleKey> : IItem<TKey> where TRoleKey : IEquatable<TRoleKey>
    {
        /// <summary>
        ///     Gets or sets the of the primary key of the role associated with this claim.
        /// </summary>
        public virtual TRoleKey RoleId { get; set; }

        /// <summary>
        ///     Gets or sets the claim type for this claim.
        /// </summary>
        public virtual string ClaimType { get; set; }

        /// <summary>
        ///     Gets or sets the claim value for this claim.
        /// </summary>
        public virtual string ClaimValue { get; set; }

        /// <summary>
        ///     Gets or sets the identifier for this role claim.
        /// </summary>
        public virtual TKey Id { get; set; }

        /// <summary>
        ///     Constructs a new claim with the type and value.
        /// </summary>
        /// <returns>The <see cref="Claim" /> that was produced.</returns>
        public virtual Claim ToClaim()
        {
            return new(ClaimType, ClaimValue);
        }

        /// <summary>
        ///     Initializes by copying ClaimType and ClaimValue from the other claim.
        /// </summary>
        /// <param name="other">The claim to initialize from.</param>
        public virtual void InitializeFromClaim(Claim other)
        {
            ClaimType = other?.Type;
            ClaimValue = other?.Value;
        }
    }
}