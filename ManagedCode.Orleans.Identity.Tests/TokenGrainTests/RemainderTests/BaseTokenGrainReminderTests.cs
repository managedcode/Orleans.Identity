using ManagedCode.Orleans.Identity.Interfaces.TokenGrains;
using ManagedCode.Orleans.Identity.Tests.Cluster;
using Xunit;
using Xunit.Abstractions;

namespace ManagedCode.Orleans.Identity.Tests.TokenGrainTests.RemainderTests;

[Collection(nameof(TestClusterApplication))]
public abstract class BaseTokenGrainReminderTests<TGrain>
    where TGrain : IBaseTokenGrain
{
    protected readonly ITestOutputHelper _outputHelper;
    protected readonly TestClusterApplication _testApp;
        
    protected BaseTokenGrainReminderTests(TestClusterApplication testApp, ITestOutputHelper outputHelper)
    {
        _outputHelper = outputHelper;
        _testApp = testApp;
    }
    
    
}