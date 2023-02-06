using ManagedCode.Orleans.Identity.Interfaces.TokenGrains;
using ManagedCode.Orleans.Identity.Tests.Cluster;
using ManagedCode.Orleans.Identity.Tests.Cluster.Grains.Interfaces.UserGrains;
using Xunit;
using Xunit.Abstractions;

namespace ManagedCode.Orleans.Identity.Tests.TokenGrainTests.UserGrainTests
{
    [Collection(nameof(TestClusterApplication))]
    public class CodeVerificationTokenUserGrainTests : BaseUserGrainsTests<ICodeVerificationTokenGrain, ICodeUserGrain>
    {
        public CodeVerificationTokenUserGrainTests(ITestOutputHelper outputHelper, TestClusterApplication testApp) : base(outputHelper, testApp)
        {
        }
    }
}
