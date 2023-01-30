using FluentAssertions;
using ManagedCode.Orleans.Identity.Interfaces.TokenGrains;
using ManagedCode.Orleans.Identity.Models;
using ManagedCode.Orleans.Identity.Tests.Cluster;
using ManagedCode.Orleans.Identity.Tests.Helpers;
using Orleans.Runtime;
using Xunit;
using Xunit.Abstractions;

namespace ManagedCode.Orleans.Identity.Tests.TokenGrainTests
{
    [Collection(nameof(TestClusterApplication))]
    public abstract class BaseTokenGrainTests<TGrain>
        where TGrain : IBaseTokenGrain
    {
        protected readonly ITestOutputHelper _outputHelper;
        protected readonly TestClusterApplication _testApp;

        protected BaseTokenGrainTests(TestClusterApplication testApp, ITestOutputHelper outputHelper)
        {
            _outputHelper = outputHelper;
            _testApp = testApp;
        }

        #region CreateToken

        [Fact]
        public virtual async Task CreateToken_WhenValueIsValid_ReturnSuccess()
        {
            // Arrange
            var createTokenModel = TokenHelper.GenerateCreateTestTokenModel();
            var tokenGrain = _testApp.Cluster.Client.GetGrain<TGrain>(createTokenModel.Value);
            
            // Act
            var result = await tokenGrain.CreateAsync(createTokenModel);
            
            // Assert
            var token = await tokenGrain.GetTokenAsync();
            result.IsSuccess.Should().BeTrue();
            token.IsSuccess.Should().BeTrue();
            token.Value.Should().NotBeNull();
            token.Value.Value.Should().Be(createTokenModel.Value);
        }
        
        [Fact]
        public virtual async Task CreateToken_WhenTokensLifetimeLessThanMinute_ReturnSuccess()
        {
            // Arrange
            var createTokenModel = TokenHelper.GenerateCreateTestTokenModel(TimeSpan.FromSeconds(30));
            var tokenGrain = _testApp.Cluster.Client.GetGrain<TGrain>(createTokenModel.Value);
            
            // Act
            var result = await tokenGrain.CreateAsync(createTokenModel);
            
            // Assert
            var token = await tokenGrain.GetTokenAsync();
            result.IsSuccess.Should().BeTrue();
            token.IsSuccess.Should().BeTrue();
            token.Value.Should().NotBeNull();
            token.Value.Value.Should().Be(createTokenModel.Value);
        }

        [Fact]
        public virtual async Task CreateToken_WhenTokensValueIsEmpty_ReturnFail()
        {
            // Arrange
            var createTokenModel = TokenHelper.GenerateCreateTestTokenModel(string.Empty);
            var tokenGrain = _testApp.Cluster.Client.GetGrain<TGrain>(createTokenModel.Value);

            // Act
            var result = await tokenGrain.CreateAsync(createTokenModel);

            // Assert
            result.IsFailed.Should().BeTrue();
        }
        
        [Fact]
        public virtual async Task CreateToken_WhenTokensValueIsNull_ReturnFail()
        {
            // Arrange
            var createTokenModel = TokenHelper.GenerateCreateTestTokenModel(null);
            var tokenGrain = _testApp.Cluster.Client.GetGrain<TGrain>(createTokenModel.Value);
            
            // Act
            var result = await tokenGrain.CreateAsync(createTokenModel);

            // Assert
            result.IsFailed.Should().BeTrue();
        }
        
        [Fact]
        public virtual async Task CreateToken_WhenTokensValueIsWhiteSpace_ReturnFail()
        {
            // Arrange
            var createTokenModel = TokenHelper.GenerateCreateTestTokenModel(" ");
            var tokenGrain = _testApp.Cluster.Client.GetGrain<TGrain>(createTokenModel.Value);
            
            // Act
            var result = await tokenGrain.CreateAsync(createTokenModel);

            // Assert
            result.IsFailed.Should().BeTrue();
        }

        [Fact]
        public virtual async Task CreateToken_WhenTokensLifetimeIsZero_ReturnFail()
        {
            // Arrange
            var createTokenModel = TokenHelper.GenerateCreateTestTokenModel(TimeSpan.Zero);
            var tokenGrain = _testApp.Cluster.Client.GetGrain<TGrain>(createTokenModel.Value);
            
            // Act
            var result = await tokenGrain.CreateAsync(createTokenModel);

            // Assert
            result.IsFailed.Should().BeTrue();
        }

        #endregion

        #region GetToken

        [Fact]
        public async Task GetToken_WhenTokenExists_ReturnSuccess()
        {
            // Arrange
            var createTokenModel = TokenHelper.GenerateCreateTestTokenModel();
            var tokenGrain = _testApp.Cluster.Client.GetGrain<TGrain>(createTokenModel.Value);
            await tokenGrain.CreateAsync(createTokenModel);
            
            // Act
            var result = await tokenGrain.GetTokenAsync();

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Value.Should().Be(createTokenModel.Value);
        }

        [Fact]
        public async Task GetToken_WhenTokenDoesntExist_ReturnFail()
        {
            // Arrange
            var randValue = Guid.NewGuid().ToString();
            var tokenGrain = _testApp.Cluster.Client.GetGrain<TGrain>(randValue);
            
            // Act
            var result = await tokenGrain.GetTokenAsync();

            // Assert
            result.IsFailed.Should().BeTrue();
        }

        #endregion

        #region VerifyToken

        [Fact]
        public async Task VerifyToken_WhenTokenExists_ReturnSuccess()
        {
            // Arrange
            var createTokenModel = TokenHelper.GenerateCreateTestTokenModel();
            var tokenGrain = _testApp.Cluster.Client.GetGrain<TGrain>(createTokenModel.Value);
            await tokenGrain.CreateAsync(createTokenModel);

            // Act
            var result = await tokenGrain.VerifyAsync();
            
            // Arrange
            result.IsSuccess.Should().BeTrue();
        }

        [Fact]
        public async Task VerifyToken_WhenTokenDoesNotExists_ReturnFail()
        {
            // Arrange 
            var randomValue = Guid.NewGuid().ToString();
            var tokenGrain = _testApp.Cluster.Client.GetGrain<TGrain>(randomValue);

            // Act
            var result = await tokenGrain.VerifyAsync();

            // Assert
            result.IsFailed.Should().BeTrue();
        }
        
        #endregion
    }
}