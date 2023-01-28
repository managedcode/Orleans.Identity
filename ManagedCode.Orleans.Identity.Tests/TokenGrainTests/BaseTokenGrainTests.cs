using FluentAssertions;
using ManagedCode.Orleans.Identity.Interfaces.TokenGrains;
using ManagedCode.Orleans.Identity.Models;
using ManagedCode.Orleans.Identity.Tests.Cluster;
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

        public BaseTokenGrainTests(TestClusterApplication testApp, ITestOutputHelper outputHelper)
        {
            _outputHelper = outputHelper;
            _testApp = testApp;
        }

        #region Token creation methods

        protected CreateTokenModel GenerateCreateTestTokenModel()
        {
            var randomValue = Guid.NewGuid().ToString();
            var randomUserGrainId = Guid.NewGuid().ToString();
            return new CreateTokenModel
            {
                Value = randomValue,
                UserGrainId = GrainId.Create("userGrain", randomUserGrainId),
                Lifetime = TimeSpan.FromMinutes(4)
            };
        }

        protected CreateTokenModel GenerateCreateTestTokenModel(string tokenValue)
        {
            var randomUserGrainId = Guid.NewGuid().ToString();
            return new CreateTokenModel
            {
                Value = tokenValue,
                UserGrainId = GrainId.Create("userGrain", randomUserGrainId),
                Lifetime = TimeSpan.FromMinutes(4)
            };
        }

        protected CreateTokenModel GenerateCreateTestTokenModel(TimeSpan lifetime)
        {
            var randomValue = Guid.NewGuid().ToString();
            var randomUserGrainId = Guid.NewGuid().ToString();
            return new CreateTokenModel
            {
                Value = randomValue,
                UserGrainId = GrainId.Create("userGrain", randomUserGrainId),
                Lifetime = lifetime
            };
        }

        protected CreateTokenModel GenerateCreateTestTokenModel(string tokenValue, TimeSpan lifetime)
        {
            var randomUserGrainId = Guid.NewGuid().ToString();
            return new CreateTokenModel
            {
                Value = tokenValue,
                UserGrainId = GrainId.Create("userGrain", randomUserGrainId),
                Lifetime = lifetime
            };
        }

        #endregion

        #region CreateToken

        [Fact]
        public virtual async Task CreateToken_WhenValueIsValid_ReturnSuccess()
        {
            // Arrange
            var createTokenModel = GenerateCreateTestTokenModel();
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
            var createTokenModel = GenerateCreateTestTokenModel(TimeSpan.FromSeconds(30));
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
            var createTokenModel = GenerateCreateTestTokenModel(string.Empty);
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
            var createTokenModel = GenerateCreateTestTokenModel(null);
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
            var createTokenModel = GenerateCreateTestTokenModel(" ");
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
            var createTokenModel = GenerateCreateTestTokenModel(TimeSpan.Zero);
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
            var createTokenModel = GenerateCreateTestTokenModel();
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
        
    }
}