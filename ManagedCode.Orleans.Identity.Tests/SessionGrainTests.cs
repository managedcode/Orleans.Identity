using FluentAssertions;
using ManagedCode.Orleans.Identity.Grains.Interfaces;
using ManagedCode.Orleans.Identity.Models;
using ManagedCode.Orleans.Identity.Tests.Cluster;
using Orleans.Runtime;
using System.Security.Claims;
using Xunit;
using Xunit.Abstractions;

namespace ManagedCode.Orleans.Identity.Tests
{
    [Collection(nameof(TestClusterApplication))]
    public class SessionGrainTests
    {
        private readonly TestClusterApplication _testApp;
        private readonly ITestOutputHelper _outputHelper;

        public SessionGrainTests(TestClusterApplication testApp, ITestOutputHelper outputHelper)
        {
            _testApp = testApp;
            _outputHelper = outputHelper;
        }

        private Dictionary<string, string> SetTestClaims(string sessionId) =>
            new Dictionary<string, string>
            {
                { ClaimTypes.MobilePhone, "+380500000" },
                { ClaimTypes.Email, "test@gmail.com" },
                { ClaimTypes.Sid, sessionId },
                { ClaimTypes.Actor, Guid.NewGuid().ToString() },
                { ClaimTypes.Role, "admin" }
            };

        private CreateSessionModel GetTestCreateSessionModel(string sessionId)
        {
            string userId = Guid.NewGuid().ToString();

            GrainId userGrainId = GrainId.Create("UserGrain", userId);
            
            CreateSessionModel createSessionModel = new CreateSessionModel
            {
                UserData = SetTestClaims(sessionId),
                UserGrainId = userGrainId
            };

            return createSessionModel;
        }
            

        [Fact]
        public async Task CreateSession_ReturnCreatedSession()
        {
            string sessionId = Guid.NewGuid().ToString();
            var createSessionModel = GetTestCreateSessionModel(sessionId);

            var sessionGrain = _testApp.Cluster.Client.GetGrain<ISessionGrain>(sessionId);

            var result = await sessionGrain.CreateAsync(createSessionModel);
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().NotBeNull();
        }

        [Fact]
        public async Task ValidateSessionGrain_ReturnClaims()
        {

        }
    }
}
