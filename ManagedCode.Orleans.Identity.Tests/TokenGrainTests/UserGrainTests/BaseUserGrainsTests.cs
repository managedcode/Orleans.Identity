using ManagedCode.Orleans.Identity.Interfaces.TokenGrains;
using ManagedCode.Orleans.Identity.Tests.Cluster;
using ManagedCode.Orleans.Identity.Tests.Helpers;
using Xunit;

namespace ManagedCode.Orleans.Identity.Tests.TokenGrainTests.UserGrainTests
{
    [Collection(nameof(TestClusterApplication))]
    public class BaseUserGrainsTests<TokenGrain>
        where TokenGrain : IBaseTokenGrain
    {
        #region Create TokenGrain methods

        protected async Task<TokenGrain> CreateTokenAsync()
        {
            var createTokenModel = TokenHelper.GenerateCreateTestTokenModel();
        }

        protected async Task<TokenGrain> CreateTokenAsync(string tokenValue)
        {
            return null;
        }

        protected async Task<TokenGrain> CreateTokenAsync(string tokenValue, TimeSpan timeSpan)
        {
            return null;
        }

        protected async Task<TokenGrain> CreateTokenAsync(TimeSpan timeSpan)
        {
            return null;
        }

        #endregion
    }

}
