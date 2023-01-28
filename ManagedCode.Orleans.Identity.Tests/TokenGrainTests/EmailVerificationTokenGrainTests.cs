using ManagedCode.Orleans.Identity.Interfaces.TokenGrains;
using ManagedCode.Orleans.Identity.Tests.Cluster;
using Xunit.Abstractions;

namespace ManagedCode.Orleans.Identity.Tests.TokenGrainTests
{
    public class EmailVerificationTokenGrainTests : BaseTokenGrainTests<IEmailVerificationTokenGrain>
    {
        public EmailVerificationTokenGrainTests(TestClusterApplication testApp, ITestOutputHelper outputHelper) : base(testApp, outputHelper)
        {
        }
    }
}
