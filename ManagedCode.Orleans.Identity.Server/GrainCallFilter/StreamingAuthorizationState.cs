using System.Security.Claims;
using Orleans.Runtime;

namespace ManagedCode.Orleans.Identity.Server.GrainCallFilter;

/// <summary>Bounded, activation-owned authority for live enumerations; never shared between grains.</summary>
internal sealed class StreamingAuthorizationState(TimeSpan retention)
{
    private const int MaximumEnumerations = 128;
    private readonly Dictionary<Guid, Entry> entries = [];

    public Entry? Find(Guid id) => entries.GetValueOrDefault(id);

    public Entry Add(Guid id, StreamingGrainCallContext.Operation operation, GrainId? sourceId, ClaimsPrincipal? principal)
    {
        var now = TimeProvider.System.GetTimestamp();
        foreach (var (key, existing) in entries)
        {
            if (!existing.InFlight && !existing.Disposing && TimeProvider.System.GetElapsedTime(existing.LastSeen, now) > retention)
            {
                entries.Remove(key);
            }
        }
        if (entries.Count >= MaximumEnumerations || entries.ContainsKey(id))
        {
            throw new UnauthorizedAccessException("Streaming authorization capacity or identity conflict.");
        }
        var entry = new Entry(operation, sourceId, principal) { InFlight = true };
        entries.Add(id, entry);
        return entry;
    }

    public bool TryBegin(Guid id, Entry entry, GrainId? caller, ClaimsPrincipal? principal, bool disposing) =>
        ReferenceEquals(Find(id), entry) && entry.TryBegin(caller, principal, disposing);

    public void Remove(Guid id, Entry entry)
    {
        if (ReferenceEquals(Find(id), entry))
        {
            entries.Remove(id);
        }
    }

    internal sealed class Entry(StreamingGrainCallContext.Operation operation, GrainId? sourceId, ClaimsPrincipal? principal)
    {
        private readonly IdentitySnapshot[] identities = principal is null
            ? []
            : principal.Identities.Select(static identity => new IdentitySnapshot(identity)).ToArray();
        public StreamingGrainCallContext.Operation Operation { get; } = operation;
        public bool InFlight { get; set; }
        public bool Disposing { get; private set; }
        public long LastSeen { get; private set; } = TimeProvider.System.GetTimestamp();
        private bool retired;

        public bool CanContinue(GrainId? caller, ClaimsPrincipal? current, bool disposing) =>
            !retired && !Disposing && (!InFlight || disposing) &&
            caller == sourceId && MatchesPrincipal(current);

        public bool TryBegin(GrainId? caller, ClaimsPrincipal? current, bool disposing)
        {
            if (!CanContinue(caller, current, disposing)) return false;
            if (disposing) Disposing = true;
            else InFlight = true;
            return true;
        }

        public bool Finish(bool disposing, bool terminated = false)
        {
            if (disposing) Disposing = false;
            else InFlight = false;
            LastSeen = TimeProvider.System.GetTimestamp();
            retired |= terminated;
            return retired && !InFlight && !Disposing;
        }

        private bool MatchesPrincipal(ClaimsPrincipal? current)
        {
            if (current is null)
            {
                return identities.Length == 0;
            }

            var index = 0;
            foreach (var identity in current.Identities)
            {
                if (index == identities.Length || !identities[index++].Matches(identity))
                {
                    return false;
                }
            }

            return index == identities.Length;
        }
    }

    private sealed class IdentitySnapshot(ClaimsIdentity identity)
    {
        private readonly string? authenticationType = identity.AuthenticationType;
        private readonly string nameClaimType = identity.NameClaimType;
        private readonly string roleClaimType = identity.RoleClaimType;
        private readonly ClaimSnapshot[] claims = identity.Claims.Select(ClaimSnapshot.FromClaim).ToArray();

        public bool Matches(ClaimsIdentity current)
        {
            if (authenticationType != current.AuthenticationType
                || nameClaimType != current.NameClaimType
                || roleClaimType != current.RoleClaimType)
            {
                return false;
            }

            var index = 0;
            foreach (var claim in current.Claims)
            {
                if (index == claims.Length || claims[index++] != ClaimSnapshot.FromClaim(claim))
                {
                    return false;
                }
            }

            return index == claims.Length;
        }
    }

    private readonly record struct ClaimSnapshot(
        string Type, string Value, string ValueType, string Issuer, string OriginalIssuer)
    {
        public static ClaimSnapshot FromClaim(Claim claim) =>
            new(claim.Type, claim.Value, claim.ValueType, claim.Issuer, claim.OriginalIssuer);
    }
}
