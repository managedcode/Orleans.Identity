using ManagedCode.Orleans.Identity.Core.Interfaces.TokenGrains;
using ManagedCode.Orleans.Identity.Tests.Cluster;
using Xunit;
using Xunit.Abstractions;

namespace ManagedCode.Orleans.Identity.Tests.TokenGrainTests.RemainderTests;

[Collection(nameof(TestClusterApplication))]
public class EmailVerificationGrainReminderTests : BaseTokenGrainReminderTests<IEmailVerificationTokenGrain>
{
    public EmailVerificationGrainReminderTests(TestClusterApplication testApp, ITestOutputHelper outputHelper) 
        : base(testApp, outputHelper)
    {
    }
}