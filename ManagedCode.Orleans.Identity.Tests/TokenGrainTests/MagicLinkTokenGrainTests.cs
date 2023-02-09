using ManagedCode.Orleans.Identity.Interfaces.TokenGrains;
using ManagedCode.Orleans.Identity.Tests.Cluster;
using Xunit;
using Xunit.Abstractions;

namespace ManagedCode.Orleans.Identity.Tests.TokenGrainTests;

[Collection(nameof(TestClusterApplication))]
public class MagicLinkTokenGrainTests : BaseTokenGrainTests<IMagicLinkTokenGrain>
{
    public MagicLinkTokenGrainTests(TestClusterApplication testApp, ITestOutputHelper outputHelper) : base(testApp, outputHelper)
    {
    }
}