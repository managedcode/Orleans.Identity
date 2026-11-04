using System.Security.Claims;
using ManagedCode.Orleans.Identity.Server.GrainCallFilter;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using Xunit;

namespace ManagedCode.Orleans.Identity.Tests;

public sealed class StreamingAuthorizationStateTests
{
    [Fact]
    public void AuthorizedDisposal_CanCancelInFlightMoveNextWithoutGrantingConcurrentReads()
    {
        var owner = Principal("owner");
        var stranger = Principal("stranger");
        var entry = new StreamingAuthorizationState.Entry(default, null, owner)
        {
            InFlight = true
        };

        entry.CanContinue(null, owner, disposing: false).ShouldBeFalse();
        entry.CanContinue(null, stranger, disposing: true).ShouldBeFalse();
        entry.TryBegin(null, owner, disposing: true).ShouldBeTrue();
        entry.Disposing.ShouldBeTrue();
        entry.CanContinue(null, owner, disposing: true).ShouldBeFalse();
        entry.CanContinue(null, owner, disposing: false).ShouldBeFalse();

        entry.Finish(disposing: true);
        entry.InFlight.ShouldBeTrue();
        entry.Disposing.ShouldBeFalse();
        entry.Finish(disposing: false);
        entry.TryBegin(null, owner, disposing: false).ShouldBeTrue();
    }

    [Fact]
    public void Continuation_PreservesIdentityOrderAndClaimOwnership()
    {
        var owner = PrincipalWithIdentities("owner", "other");
        var equivalent = PrincipalWithIdentities("owner", "other");
        var reordered = PrincipalWithIdentities("other", "owner");
        var reassigned = new ClaimsPrincipal([
            new ClaimsIdentity([
                new Claim(ClaimTypes.NameIdentifier, "owner"),
                new Claim(ClaimTypes.NameIdentifier, "other")
            ], "stream-test"),
            new ClaimsIdentity([], "stream-test")
        ]);
        var entry = new StreamingAuthorizationState.Entry(default, null, owner);

        entry.CanContinue(null, equivalent, disposing: false).ShouldBeTrue();
        entry.CanContinue(null, reordered, disposing: false).ShouldBeFalse();
        entry.CanContinue(null, reassigned, disposing: true).ShouldBeFalse();
    }

    [Fact]
    public void Continuation_BindsClaimPropertiesToTheOpeningPrincipal()
    {
        var owner = Principal("owner");
        var claim = owner.Claims.Single();
        claim.Properties["tenant"] = "tenant-1";
        claim.Properties["source"] = "issuer";
        var entry = new StreamingAuthorizationState.Entry(default, null, owner);

        var equivalent = Principal("owner");
        var equivalentClaim = equivalent.Claims.Single();
        equivalentClaim.Properties["source"] = "issuer";
        equivalentClaim.Properties["tenant"] = "tenant-1";
        entry.CanContinue(null, equivalent, disposing: false).ShouldBeTrue();

        claim.Properties["tenant"] = "tenant-2";
        entry.CanContinue(null, owner, disposing: false).ShouldBeFalse();
        entry.CanContinue(null, owner, disposing: true).ShouldBeFalse();
    }

    [Fact]
    public void StaleEntry_CannotBeginOrRemoveReplacementWithSameId()
    {
        using var services = new ServiceCollection().AddSingleton<TimeProvider>(new ManualClock()).BuildServiceProvider();
        var state = new StreamingAuthorizationState(TimeSpan.FromMinutes(1), services.GetRequiredService<TimeProvider>());
        var id = Guid.NewGuid();
        var owner = Principal("owner");
        var original = new StreamingAuthorizationState.Entry(default, null, owner);
        state.Add(id, original);
        state.Finish(id, original, disposing: false, terminated: false);
        state.Remove(id, original);
        var replacement = new StreamingAuthorizationState.Entry(default, null, Principal("replacement"));
        state.Add(id, replacement);

        state.TryBegin(id, original, null, owner, disposing: false).ShouldBeFalse();
        state.Remove(id, original);
        state.Find(id).ShouldBeSameAs(replacement);
    }

    [Fact]
    public void RetentionUsesInjectedMonotonicClockAfterOperationFinishes()
    {
        var clock = new ManualClock();
        using var services = new ServiceCollection().AddSingleton<TimeProvider>(clock).BuildServiceProvider();
        var retention = TimeSpan.FromMinutes(1);
        var state = new StreamingAuthorizationState(retention, services.GetRequiredService<TimeProvider>());
        var id = Guid.NewGuid();
        var entry = new StreamingAuthorizationState.Entry(default, null, null);
        state.Add(id, entry);

        clock.Advance(retention + retention);
        state.Add(Guid.NewGuid(), new StreamingAuthorizationState.Entry(default, null, null));
        state.Find(id).ShouldBeSameAs(entry);

        state.Finish(id, entry, disposing: false, terminated: false);
        clock.Advance(retention);
        state.Add(Guid.NewGuid(), new StreamingAuthorizationState.Entry(default, null, null));
        state.Find(id).ShouldBeSameAs(entry);

        clock.Advance(TimeSpan.FromTicks(1));
        state.Add(Guid.NewGuid(), new StreamingAuthorizationState.Entry(default, null, null));
        state.Find(id).ShouldBeNull();
    }

    [Fact]
    public void TerminatedEntry_WaitsForBothInFlightOperationsBeforeRemoval()
    {
        var owner = Principal("owner");
        var entry = new StreamingAuthorizationState.Entry(default, null, owner) { InFlight = true };
        entry.TryBegin(null, owner, disposing: true).ShouldBeTrue();

        entry.Finish(disposing: true, terminated: true).ShouldBeFalse();
        entry.CanContinue(null, owner, disposing: true).ShouldBeFalse();
        entry.Finish(disposing: false).ShouldBeTrue();
    }

    [Fact]
    public void Continuation_MatchesIteratorBackedIdentitiesAndClaims()
    {
        var entry = new StreamingAuthorizationState.Entry(default, null, Principal("owner"));
        var identity = new IteratorIdentity("owner");
        var principal = new IteratorPrincipal(identity);

        entry.CanContinue(null, principal, disposing: false).ShouldBeTrue();

        identity.AddClaim(new Claim(ClaimTypes.Role, "additional"));
        entry.CanContinue(null, principal, disposing: false).ShouldBeFalse();
    }

    private sealed class ManualClock : TimeProvider
    {
        private long timestamp;

        public override long TimestampFrequency => TimeSpan.TicksPerSecond;

        public override long GetTimestamp() => timestamp;

        public void Advance(TimeSpan interval) => timestamp += interval.Ticks;
    }

    private sealed class IteratorPrincipal(ClaimsIdentity identity) : ClaimsPrincipal(identity)
    {
        public override IEnumerable<ClaimsIdentity> Identities => base.Identities.Where(static _ => true);
    }

    private sealed class IteratorIdentity(string name)
        : ClaimsIdentity([new Claim(ClaimTypes.NameIdentifier, name)], "stream-test")
    {
        public override IEnumerable<Claim> Claims => base.Claims.Where(static _ => true);
    }

    private static ClaimsPrincipal PrincipalWithIdentities(string first, string second) =>
        new([Principal(first).Identities.Single(), Principal(second).Identities.Single()]);

    private static ClaimsPrincipal Principal(string name) =>
        new(new ClaimsIdentity([new Claim(ClaimTypes.NameIdentifier, name)], "stream-test"));
}
