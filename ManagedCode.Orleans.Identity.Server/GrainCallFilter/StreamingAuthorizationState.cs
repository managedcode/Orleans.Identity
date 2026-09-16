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
        foreach (var expired in entries.Where(pair => !pair.Value.InFlight && now - pair.Value.LastSeen > retention)
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
        public DateTimeOffset LastSeen { get; set; } = TimeProvider.System.GetUtcNow();

        public bool Matches(GrainId? caller, ClaimsPrincipal? current) =>
            caller == sourceId && identity.SequenceEqual(Identity(current), StringComparer.Ordinal);

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
