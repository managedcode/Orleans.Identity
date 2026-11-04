using System.Security.Claims;
using Orleans.Runtime;

namespace ManagedCode.Orleans.Identity.Server.GrainCallFilter;

/// <summary>Bounded, activation-owned authority for live enumerations; never shared between grains.</summary>
internal sealed class StreamingAuthorizationState(TimeSpan retention, TimeProvider timeProvider)
{
    private const int MaximumEnumerations = 128;
    private readonly Dictionary<Guid, Entry> entries = [];

    public Entry? Find(Guid id) => entries.GetValueOrDefault(id);

    public void Add(Guid id, Entry entry)
    {
        if (entries.Count != 0)
        {
            var now = timeProvider.GetTimestamp();
            foreach (var (key, existing) in entries)
            {
                if (!existing.InFlight && !existing.Disposing && timeProvider.GetElapsedTime(existing.LastSeen, now) > retention)
                {
                    entries.Remove(key);
                }
            }
        }

        if (entries.Count >= MaximumEnumerations || !entries.TryAdd(id, entry))
        {
            throw new UnauthorizedAccessException("Streaming authorization capacity or identity conflict.");
        }

        entry.InFlight = true;
    }

    public bool TryBegin(Guid id, Entry entry, GrainId? caller, ClaimsPrincipal? principal, bool disposing) =>
        ReferenceEquals(Find(id), entry) && entry.TryBegin(caller, principal, disposing);

    public void Finish(Guid id, Entry entry, bool disposing, bool terminated)
    {
        if (entry.Finish(disposing, terminated))
        {
            Remove(id, entry);
        }
        else
        {
            entry.LastSeen = timeProvider.GetTimestamp();
        }
    }

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
        public long LastSeen { get; set; }
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
            retired |= terminated;
            return retired && !InFlight && !Disposing;
        }

        private bool MatchesPrincipal(ClaimsPrincipal? current)
        {
            if (current is null)
            {
                return identities.Length == 0;
            }

            var currentIdentities = current.Identities;
            if (currentIdentities is IReadOnlyList<ClaimsIdentity> list)
            {
                if (identities.Length != list.Count) return false;
                for (var i = 0; i < identities.Length; i++)
                {
                    if (!identities[i].Matches(list[i])) return false;
                }
                return true;
            }

            var index = 0;
            foreach (var identity in currentIdentities)
            {
                if (index == identities.Length || !identities[index++].Matches(identity))
                {
                    return false;
                }
            }

            return index == identities.Length;
        }
    }

    private readonly struct IdentitySnapshot(ClaimsIdentity identity)
    {
        private readonly string? authenticationType = identity.AuthenticationType;
        private readonly string nameClaimType = identity.NameClaimType;
        private readonly string roleClaimType = identity.RoleClaimType;
        private readonly ClaimSnapshot[] claims = SnapshotClaims(identity.Claims);
        private readonly ActorSnapshot? actor = identity.Actor is { } delegated ? new ActorSnapshot(delegated) : null;

        private static ClaimSnapshot[] SnapshotClaims(IEnumerable<Claim> claims)
        {
            if (claims is not IReadOnlyList<Claim> list)
            {
                return claims.Select(static claim => new ClaimSnapshot(claim)).ToArray();
            }

            if (list.Count == 0)
            {
                return [];
            }

            var snapshots = new ClaimSnapshot[list.Count];
            for (var i = 0; i < snapshots.Length; i++)
            {
                snapshots[i] = new ClaimSnapshot(list[i]);
            }

            return snapshots;
        }

        public bool Matches(ClaimsIdentity current)
        {
            if (authenticationType != current.AuthenticationType
                || nameClaimType != current.NameClaimType
                || roleClaimType != current.RoleClaimType)
            {
                return false;
            }

            var currentActor = current.Actor;
            if (actor is null ? currentActor is not null : currentActor is null || !actor.Matches(currentActor))
            {
                return false;
            }

            var currentClaims = current.Claims;
            if (currentClaims is IReadOnlyList<Claim> list)
            {
                if (claims.Length != list.Count) return false;
                for (var i = 0; i < claims.Length; i++)
                {
                    if (!claims[i].Matches(list[i])) return false;
                }
                return true;
            }

            var index = 0;
            foreach (var claim in currentClaims)
            {
                if (index == claims.Length || !claims[index++].Matches(claim))
                {
                    return false;
                }
            }

            return index == claims.Length;
        }
    }

    private sealed class ActorSnapshot(ClaimsIdentity actor)
    {
        private readonly IdentitySnapshot snapshot = new(actor);

        public bool Matches(ClaimsIdentity current) => snapshot.Matches(current);
    }

    private readonly struct ClaimSnapshot(Claim claim)
    {
        private readonly string type = claim.Type;
        private readonly string value = claim.Value;
        private readonly string valueType = claim.ValueType;
        private readonly string issuer = claim.Issuer;
        private readonly string originalIssuer = claim.OriginalIssuer;
        private readonly KeyValuePair<string, string>[] properties = claim.Properties.ToArray();

        public bool Matches(Claim current)
        {
            if (type != current.Type || value != current.Value || valueType != current.ValueType
                || issuer != current.Issuer || originalIssuer != current.OriginalIssuer)
            {
                return false;
            }

            var currentProperties = current.Properties;
            if (properties.Length != currentProperties.Count)
            {
                return false;
            }

            foreach (var (key, expected) in properties)
            {
                if (!currentProperties.TryGetValue(key, out var actual) || expected != actual)
                {
                    return false;
                }
            }

            return true;
        }
    }
}
