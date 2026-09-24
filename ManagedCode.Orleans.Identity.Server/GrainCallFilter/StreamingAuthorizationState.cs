using System.Security.Claims;
using Orleans.Runtime;
using Orleans.Serialization.Invocation;

namespace ManagedCode.Orleans.Identity.Server.GrainCallFilter;

/// <summary>Bounded, activation-owned authority for live enumerations; never shared between grains.</summary>
internal sealed class StreamingAuthorizationState(TimeSpan retention)
{
    private const int MaximumEnumerations = 128;
    private readonly Dictionary<Guid, Entry> entries = [];

    public Entry? Find(Guid id) => entries.GetValueOrDefault(id);

    public Entry Add(Guid id, IInvokable request, GrainId? sourceId, ClaimsPrincipal? principal)
    {
        var now = TimeProvider.System.GetUtcNow();
        foreach (var expired in entries.Where(pair => !pair.Value.InFlight && !pair.Value.Disposing && now - pair.Value.LastSeen > retention)
                     .Select(pair => pair.Key).ToArray())
        {
            entries.Remove(expired);
        }
        if (entries.Count >= MaximumEnumerations || entries.ContainsKey(id))
        {
            throw new UnauthorizedAccessException("Streaming authorization capacity or identity conflict.");
        }
        var entry = new Entry(request, sourceId, principal) { InFlight = true };
        entries.Add(id, entry);
        return entry;
    }

    public void Remove(Guid id) => entries.Remove(id);

    internal sealed class Entry(IInvokable request, GrainId? sourceId, ClaimsPrincipal? principal)
    {
        private readonly string[] identity = Identity(principal);
        public IInvokable Request { get; } = request;
        public bool InFlight { get; set; }
        public bool Disposing { get; private set; }
        public DateTimeOffset LastSeen { get; private set; } = TimeProvider.System.GetUtcNow();

        public bool CanContinue(GrainId? caller, ClaimsPrincipal? current, bool disposing) =>
            !Disposing && (!InFlight || disposing) &&
            caller == sourceId && identity.SequenceEqual(Identity(current), StringComparer.Ordinal);

        public bool TryBegin(GrainId? caller, ClaimsPrincipal? current, bool disposing)
        {
            if (!CanContinue(caller, current, disposing)) return false;
            if (disposing) Disposing = true;
            else InFlight = true;
            return true;
        }

        public void Finish(bool disposing)
        {
            if (disposing) Disposing = false;
            else InFlight = false;
            LastSeen = TimeProvider.System.GetUtcNow();
        }

        private static string[] Identity(ClaimsPrincipal? value) => value is null ? [] :
            value.Identities.SelectMany(identity => new[]
                {
                    System.Text.Json.JsonSerializer.Serialize(new[]
                    {
                        identity.AuthenticationType, identity.NameClaimType, identity.RoleClaimType
                    })
                }.Concat(identity.Claims.Select(claim =>
                    System.Text.Json.JsonSerializer.Serialize(new[]
                    {
                        identity.AuthenticationType, identity.NameClaimType, identity.RoleClaimType,
                        claim.Type, claim.Value, claim.ValueType, claim.Issuer, claim.OriginalIssuer
                    }))))
                .Order(StringComparer.Ordinal).ToArray();
    }
}
