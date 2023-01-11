using ManagedCode.Orleans.Identity.Grains.Interfaces;
using ManagedCode.Orleans.Identity.Models;
using ManagedCode.Orleans.Identity.Tests.Cluster;
using Xunit;
using Xunit.Abstractions;

namespace ManagedCode.Orleans.Identity.Tests;

[Collection(nameof(TestClusterApplication))]
public class ControllerTests
{
    private readonly TestClusterApplication _testApp;
    private readonly ITestOutputHelper _outputHelper;

    public ControllerTests(TestClusterApplication testApp, ITestOutputHelper outputHelper)
    {
        _testApp = testApp;
        _outputHelper = outputHelper;
    }

    private async Task CreateSession()
    {

    }

    [Fact]
    public async Task CreateSession_ReturnOk()
    {
    }
}