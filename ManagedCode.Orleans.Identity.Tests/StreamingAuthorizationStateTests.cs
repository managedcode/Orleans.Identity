using System.Security.Claims;
using ManagedCode.Orleans.Identity.Server.GrainCallFilter;
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
        var entry = new StreamingAuthorizationState.Entry(null!, null, owner)
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

    private static ClaimsPrincipal Principal(string name) =>
        new(new ClaimsIdentity([new Claim(ClaimTypes.NameIdentifier, name)], "stream-test"));
}
