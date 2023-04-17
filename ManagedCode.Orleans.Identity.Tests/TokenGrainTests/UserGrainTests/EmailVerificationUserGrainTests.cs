using ManagedCode.Orleans.Identity.Core.Interfaces.TokenGrains;
using ManagedCode.Orleans.Identity.Core.Interfaces.UserGrains;
using ManagedCode.Orleans.Identity.Tests.Cluster;
using ManagedCode.Orleans.Identity.Tests.Cluster.Grains.Interfaces.UserGrains;
using Xunit;
using Xunit.Abstractions;

namespace ManagedCode.Orleans.Identity.Tests.TokenGrainTests.UserGrainTests
{
    [Collection(nameof(TestClusterApplication))]
    public class EmailVerificationUserGrainTests : BaseUserGrainsTests<IEmailVerificationTokenGrain, ISocialUserGrain>
    {
        public EmailVerificationUserGrainTests(ITestOutputHelper outputHelper, TestClusterApplication testApp) : base(outputHelper, testApp)
        {
        }
    }
}
