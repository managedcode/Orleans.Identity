using ManagedCode.Orleans.Identity.Interfaces.TokenGrains;
using ManagedCode.Orleans.Identity.Models;
using ManagedCode.Orleans.Identity.Tests.Cluster;
using ManagedCode.Orleans.Identity.Tests.Helpers;
using Xunit;
using Xunit.Abstractions;

namespace ManagedCode.Orleans.Identity.Tests.TokenGrainTests.UserGrainTests
{
    [Collection(nameof(TestClusterApplication))]
    public class BaseUserGrainsTests<TTokenGrain>
        where TTokenGrain : IBaseTokenGrain
    {
        protected readonly ITestOutputHelper _outputHelper;
        protected readonly TestClusterApplication _testApp;

        public BaseUserGrainsTests(ITestOutputHelper outputHelper, TestClusterApplication testApp)
        {
            _outputHelper = outputHelper;
            _testApp = testApp;
        }

        #region Create TokenGrain methods

        private async ValueTask<TTokenGrain> CreateAndGetTokenGrainAsync(CreateTokenModel createTokenModel)
        {
            var tokenGrain = _testApp.Cluster.Client.GetGrain<TTokenGrain>(createTokenModel.Value);
            await tokenGrain.CreateAsync(createTokenModel);
            return tokenGrain;
        }

        protected async Task<TTokenGrain> CreateTokenAsync()
        {
            var createTokenModel = TokenHelper.GenerateCreateTestTokenModel();
            var tokenGrain = await CreateAndGetTokenGrainAsync(createTokenModel);
            return tokenGrain;
        }
        
        protected async Task<TTokenGrain> CreateTokenAsync(string tokenValue)
        {
            var createTokenModel = TokenHelper.GenerateCreateTestTokenModel(tokenValue);
            var tokenGrain = await CreateAndGetTokenGrainAsync(createTokenModel);
            return tokenGrain;
        }

        protected async Task<TTokenGrain> CreateTokenAsync(string tokenValue, TimeSpan timeSpan)
        {
            var createTokenModel = TokenHelper.GenerateCreateTestTokenModel(tokenValue, timeSpan);
            var tokenGrain = await CreateAndGetTokenGrainAsync(createTokenModel);
            return tokenGrain;
        }

        protected async Task<TTokenGrain> CreateTokenAsync(TimeSpan timeSpan)
        {
            var createTokenModel = TokenHelper.GenerateCreateTestTokenModel(timeSpan);
            var tokenGrain = await CreateAndGetTokenGrainAsync(createTokenModel);
            return tokenGrain;
        }

        #endregion
    }

}
