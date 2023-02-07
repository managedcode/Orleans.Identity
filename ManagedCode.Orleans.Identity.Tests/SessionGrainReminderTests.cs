using FluentAssertions;
using ManagedCode.Orleans.Identity.Enums;
using ManagedCode.Orleans.Identity.Interfaces;
using ManagedCode.Orleans.Identity.Models;
using ManagedCode.Orleans.Identity.Tests.Cluster;
using ManagedCode.Orleans.Identity.Tests.Cluster.ShortLifetimeSilo;
using ManagedCode.Orleans.Identity.Tests.Helpers;
using Orleans.Runtime;
using Xunit;
using Xunit.Abstractions;

namespace ManagedCode.Orleans.Identity.Tests;

[Collection(nameof(ShortLifetimeSiloTestApp))]
public class SessionGrainReminderTests 
{
    private readonly ITestOutputHelper _outputHelper;
    private readonly ShortLifetimeSiloTestApp _testApp;

    private CreateSessionModel GetTestCreateSessionModel(string sessionId)
    {
        var userId = Guid.NewGuid().ToString();

        var userGrainId = GrainId.Create("UserGrain", userId);

        var createSessionModel = new CreateSessionModel
        {
            UserData = SessionHelper.SetTestClaims(sessionId),
            UserGrainId = userGrainId
        };

        return createSessionModel;
    }
    
    public SessionGrainReminderTests(ITestOutputHelper outputHelper, ShortLifetimeSiloTestApp testApp)
    {
        _outputHelper = outputHelper;
        _testApp = testApp;
    }

    [Fact]
    public async Task DeactivateGrain_RegisterReminder_CloseSession()
    {
        // Arrange
        var sessionId = Guid.NewGuid().ToString();
        var sessionCreateModel = GetTestCreateSessionModel(sessionId);
        var sessionGrain = _testApp.Cluster.GrainFactory.GetGrain<ISessionGrain>(sessionId);
        await sessionGrain.CreateAsync(sessionCreateModel);

        // Act
        await Task.Delay(TimeSpan.FromMinutes(5));
        var result = await sessionGrain.GetSessionAsync();

        // Assert
        result.IsFailed.Should().BeTrue();
    }

    [Fact]
    public async Task DeactivateGrain_WhenClearSessionOnCloseIsFalse_RegisterReminder_CloseSession()
    {
        // Arrange
        ShortLifetimeSiloOptions.SessionOption.ClearStateOnClose = false;
        var sessionId = Guid.NewGuid().ToString();
        var sessionCreateModel = GetTestCreateSessionModel(sessionId);
        var sessionGrain = _testApp.Cluster.GrainFactory.GetGrain<ISessionGrain>(sessionId);
        await sessionGrain.CreateAsync(sessionCreateModel);

        // Act
        await Task.Delay(TimeSpan.FromMinutes(5));
        var result = await sessionGrain.GetSessionAsync();

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Status.Should().Be(SessionStatus.Closed);
        result.Value.ClosedDate.Should().NotBeNull();
        ShortLifetimeSiloOptions.SessionOption.ClearStateOnClose = true;
    }

    [Fact]
    public async Task DeactivateGrain_WhenSessionDoesntExists_DoNotRegisterReminder()
    {
        // Arrange
        var sessionId = Guid.NewGuid().ToString();
        var sessionGrain = _testApp.Cluster.GrainFactory.GetGrain<ISessionGrain>(sessionId);

        // Act
        var result = await sessionGrain.PauseSessionAsync();

        // Assert
        result.IsFailed.Should().BeTrue();
    }

    [Fact]
    public async Task DeactivateGrain_RegisterReminder_CloseSession_UnregisterReminder()
    {
        // Arrange
        ShortLifetimeSiloOptions.SessionOption.SessionLifetime = TimeSpan.FromMinutes(4); 
        var sessionId = Guid.NewGuid().ToString();
        var sessionCreateModel = GetTestCreateSessionModel(sessionId);
        var sessionGrain = _testApp.Cluster.GrainFactory.GetGrain<ISessionGrain>(sessionId);
        await sessionGrain.CreateAsync(sessionCreateModel);
        
        // Act
        await sessionGrain.GetSessionAsync();
        await Task.Delay(TimeSpan.FromMinutes(2));
        var result = await sessionGrain.CloseAsync();

        // Assert
        result.IsSuccess.Should().BeTrue();
        ShortLifetimeSiloOptions.SessionOption.SessionLifetime = TimeSpan.FromMinutes(1).Add(TimeSpan.FromSeconds(40));
    }

    [Fact]
    public async Task DeactivateGrain_ReactivateGrainWhenSessionLifetimeIsExpired_CloseSession_UnregisterReminder()
    {
        // Arrange
        var sessionId = Guid.NewGuid().ToString();
        var sessionCreateModel = GetTestCreateSessionModel(sessionId);
        var sessionGrain = _testApp.Cluster.GrainFactory.GetGrain<ISessionGrain>(sessionId);
        await sessionGrain.CreateAsync(sessionCreateModel);
        
        // Act
        await Task.Delay(TimeSpan.FromMinutes(3));
        var result = await sessionGrain.GetSessionAsync();
        
        // Assert
        result.IsFailed.Should().BeTrue();
    }

    [Fact]
    public async Task DeactivateGrain_ReactivateGrainWhenSessionLifetimeIsExpiredAndClearStateOnClose_CloseSession_UnregisterReminder_ReturnClosedSession()
    {
        // Arrange
        ShortLifetimeSiloOptions.SessionOption.ClearStateOnClose = false;
        var sessionId = Guid.NewGuid().ToString();
        var sessionCreateModel = GetTestCreateSessionModel(sessionId);
        var sessionGrain = _testApp.Cluster.GrainFactory.GetGrain<ISessionGrain>(sessionId);
        await sessionGrain.CreateAsync(sessionCreateModel);
        
        // Act
        await Task.Delay(TimeSpan.FromMinutes(3));
        var result = await sessionGrain.GetSessionAsync();
        
        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Status.Should().Be(SessionStatus.Closed);
        ShortLifetimeSiloOptions.SessionOption.ClearStateOnClose = true;
    }
}