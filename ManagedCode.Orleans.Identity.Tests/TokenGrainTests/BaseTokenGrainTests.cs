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
            var createTokenModel = GenerateCreateTestTokenModel();

            var tokenGrain = _testApp.Cluster.Client.GetGrain<TGrain>(createTokenModel.Value);
            var result = await tokenGrain.CreateAsync(createTokenModel);
            var token = await tokenGrain.GetTokenAsync();

            result.IsSuccess.Should().BeTrue();
            token.IsSuccess.Should().BeTrue();
            token.Value.Should().NotBeNull();
            token.Value.Value.Should().Be(createTokenModel.Value);
        }

        [Fact]
        public virtual async Task CreateToken_WhenTokensValueIsEmpty_ReturnFail()
        {
            var createTokenModel = GenerateCreateTestTokenModel(string.Empty);

            var tokenGrain = _testApp.Cluster.Client.GetGrain<TGrain>(createTokenModel.Value);
            var result = await tokenGrain.CreateAsync(createTokenModel);

            result.IsFailed.Should().BeTrue();
        }
        
        [Fact]
        public virtual async Task CreateToken_WhenTokensValueIsNull_ReturnFail()
        {
            var createTokenModel = GenerateCreateTestTokenModel(null);

            var tokenGrain = _testApp.Cluster.Client.GetGrain<TGrain>(createTokenModel.Value);
            var result = await tokenGrain.CreateAsync(createTokenModel);

            result.IsFailed.Should().BeTrue();
        }
        
        [Fact]
        public virtual async Task CreateToken_WhenTokensValueIsWhiteSpace_ReturnFail()
        {
            var createTokenModel = GenerateCreateTestTokenModel(" ");

            var tokenGrain = _testApp.Cluster.Client.GetGrain<TGrain>(createTokenModel.Value);
            var result = await tokenGrain.CreateAsync(createTokenModel);

            result.IsFailed.Should().BeTrue();
        }

        #endregion
    }
}