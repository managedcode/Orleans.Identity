using FluentAssertions;
using ManagedCode.Orleans.Identity.Interfaces.TokenGrains;
using ManagedCode.Orleans.Identity.Tests.Cluster;
using ManagedCode.Orleans.Identity.Tests.Helpers;
using Xunit;
using Xunit.Abstractions;

namespace ManagedCode.Orleans.Identity.Tests.TokenGrainTests.RemainderTests;

[Collection(nameof(TestClusterApplication))]
public abstract class BaseTokenGrainReminderTests<TGrain>
    where TGrain : IBaseTokenGrain
{
    protected readonly ITestOutputHelper _outputHelper;
    protected readonly TestClusterApplication _testApp;
        
    protected BaseTokenGrainReminderTests(TestClusterApplication testApp, ITestOutputHelper outputHelper)
    {
        _outputHelper = outputHelper;
        _testApp = testApp;
    }

    #region Reminder

    [Fact]
    public async Task ExecuteReminder_WhenTokenExists_DeleteToken()
    {
        // Arrange
        var createTokenModel = TokenHelper.GenerateCreateTestTokenModel();
        var tokenGrain = _testApp.Cluster.Client.GetGrain<TGrain>(createTokenModel.Value);
        await tokenGrain.CreateAsync(createTokenModel);
            
        // Act
        await Task.Delay(TimeSpan.FromMinutes(3));

        // Assert
        var result = await tokenGrain.VerifyAsync();
        result.IsFailed.Should().BeTrue();
    }

    #endregion

    #region Timer

    [Fact]
    public async Task ExecuteTimer_WhenTokenExists_DeleteToken()
    {
        // Arrange
        var createTokenModel = TokenHelper.GenerateCreateTestTokenModel(TimeSpan.FromSeconds(30));
        var tokenGrain = _testApp.Cluster.Client.GetGrain<TGrain>(createTokenModel.Value);
        await tokenGrain.CreateAsync(createTokenModel);
            
        // Act
        await Task.Delay(TimeSpan.FromMinutes(2));

        // Assert
        var result = await tokenGrain.VerifyAsync();
        result.IsFailed.Should().BeTrue();
    }

    #endregion
}