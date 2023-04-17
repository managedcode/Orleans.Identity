using ManagedCode.Orleans.Identity.Core.Interfaces.TokenGrains;
using ManagedCode.Orleans.Identity.Tests.Cluster;
using Xunit;
using Xunit.Abstractions;

namespace ManagedCode.Orleans.Identity.Tests.TokenGrainTests
{
    [Collection(nameof(TestClusterApplication))]
    public class EmailVerificationTokenGrainTests : BaseTokenGrainTests<IEmailVerificationTokenGrain>
    {
        public EmailVerificationTokenGrainTests(TestClusterApplication testApp, ITestOutputHelper outputHelper) : base(testApp, outputHelper)
        {
        }
    }
}
