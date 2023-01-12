using FluentAssertions;
using ManagedCode.Orleans.Identity.Grains.Interfaces;
using ManagedCode.Orleans.Identity.Models;
using ManagedCode.Orleans.Identity.Shared.Enums;
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

        #region CreateSession

        [Fact]
        public async Task CreateSessionAsync_ReturnCreatedSession()
        {
            // Arrange
            string sessionId = Guid.NewGuid().ToString();
            var createSessionModel = GetTestCreateSessionModel(sessionId);
            var sessionGrain = _testApp.Cluster.Client.GetGrain<ISessionGrain>(sessionId);

            // Act
            var result = await sessionGrain.CreateAsync(createSessionModel);
            
            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().NotBeNull();
        }

        #endregion

        #region ValidateSessionAndGetClaimsAsync
        
        [Fact]
        public async Task ValidateSessionAndGetClaimsAsync_ReturnClaims()
        {
            // Arrange
            var sessionId = Guid.NewGuid().ToString();
            var createSessionModel = GetTestCreateSessionModel(sessionId);
            var sessionGrain = _testApp.Cluster.Client.GetGrain<ISessionGrain>(sessionId);
            await sessionGrain.CreateAsync(createSessionModel);

            // Act
            var result = await sessionGrain.ValidateAndGetClaimsAsync();

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().NotBeNull();
            result.Value.Count.Should().Be(createSessionModel.UserData.Count);
        }

        [Fact]
        public async Task ValidateSessionAndGetClaimsAsync_WhenSessionStateIsNotExists_ReturnFail()
        {
            // Arrange
            var sessionId = Guid.NewGuid().ToString();
            var sessionGrain = _testApp.Cluster.Client.GetGrain<ISessionGrain>(sessionId);

            // Act
            var result = await sessionGrain.ValidateAndGetClaimsAsync();

            // Assert
            result.IsFailed.Should().BeTrue();
            result.Value.Should().BeNull();
        }

        [Fact]
        public async Task ValidateSessionAndGetClaimsAsync_WhenSessionIsNotActive_ReturnFail()
        {
            // Arrange
            var sessionId = Guid.NewGuid().ToString();
            var createSessionModel = GetTestCreateSessionModel(sessionId);
            var sessionGrain = _testApp.Cluster.Client.GetGrain<ISessionGrain>(sessionId);
            TestSiloOptions.SessionOption.ClearStateOnClose = false;
            await sessionGrain.CreateAsync(createSessionModel);
            await sessionGrain.CloseAsync();
            
            // Act
            var result = await sessionGrain.ValidateAndGetClaimsAsync();

            // Assert
            result.IsFailed.Should().BeTrue();
            result.Value.Should().BeNull();
            TestSiloOptions.SessionOption.ClearStateOnClose = false;
        }

        #endregion

        #region PauseSessionAsync
        
        [Fact]
        public async Task PauseSessionAsync_ReturnSuccess()
        {
            // Arrange
            var sessionId = Guid.NewGuid().ToString();
            var createSessionModel = GetTestCreateSessionModel(sessionId);
            var sessionGrain = _testApp.Cluster.Client.GetGrain<ISessionGrain>(sessionId);
            await sessionGrain.CreateAsync(createSessionModel);

            // Act
            var result = await sessionGrain.PauseSessionAsync();
            
            // Assert
            var session = await sessionGrain.GetSessionAsync();
            result.IsSuccess.Should().BeTrue();
            session.Value.Status.Should().Be(SessionStatus.Paused);
        }

        [Fact]
        public async Task PauseSessionAsync_WhenSessionStateIsNotExists_ReturnFailed()
        {
            // Arrange
            var sessionId = Guid.NewGuid().ToString();
            var sessionGrain = _testApp.Cluster.Client.GetGrain<ISessionGrain>(sessionId);

            // Act
            var result = await sessionGrain.PauseSessionAsync();

            // Assert
            result.IsFailed.Should().BeTrue();
        }
        
        #endregion

        #region CloseSessionAsync
        
        [Fact]
        public async Task CloseSessionAsync_WhenSessionExistsAndClearStateIsTrue_ReturnSuccessAndClearState()
        {
            // Arrange
            var sessionId = Guid.NewGuid().ToString();
            var createSessionModel = GetTestCreateSessionModel(sessionId);
            var sessionGrain = _testApp.Cluster.Client.GetGrain<ISessionGrain>(sessionId);
            await sessionGrain.CreateAsync(createSessionModel);   
            
            // Act
            var result = await sessionGrain.CloseAsync();
            var session = await sessionGrain.GetSessionAsync();
                
            // Arrange
            result.IsSuccess.Should().BeTrue();
            session.IsFailed.Should().BeTrue();
        }
        
        [Fact]
        public async Task CloseSessionAsync_WhenSessionExistsAndClearStateIsFalse_ReturnSuccess()
        {
            // Arrange
            var sessionId = Guid.NewGuid().ToString();
            var createSessionModel = GetTestCreateSessionModel(sessionId);
            var sessionGrain = _testApp.Cluster.Client.GetGrain<ISessionGrain>(sessionId);
            await sessionGrain.CreateAsync(createSessionModel);
            TestSiloOptions.SessionOption.ClearStateOnClose = false;
            
            // Act
            var result = await sessionGrain.CloseAsync();
            var session = await sessionGrain.GetSessionAsync();
            
            // Arrange
            result.IsSuccess.Should().BeTrue();
            session.IsSuccess.Should().BeTrue();
            session.Value.Should().NotBeNull();
            session.Value.Status.Should().Be(SessionStatus.Closed);
            
            TestSiloOptions.SessionOption.ClearStateOnClose = true;
        }

        [Fact]
        public async Task CloseSessionAsync_WhenSessionIsNotExists_ReturnFail()
        {
            // Arrange
            var sessionId = Guid.NewGuid().ToString();
            var createSessionModel = GetTestCreateSessionModel(sessionId);
            var sessionGrain = _testApp.Cluster.Client.GetGrain<ISessionGrain>(sessionId);
            
            // Act
            var result = await sessionGrain.CloseAsync();

            // Assert
            result.IsFailed.Should().BeTrue();
        }
        
        #endregion

        #region ResumeSessionAsync
        
        [Fact]
        public async Task ResumeSessionAsync_WhenSessionExists_ReturnSuccess()
        {
            // Arrange
            var sessionId = Guid.NewGuid().ToString();
            var createSessionModel = GetTestCreateSessionModel(sessionId);
            var sessionGrain = _testApp.Cluster.Client.GetGrain<ISessionGrain>(sessionId);
            await sessionGrain.CreateAsync(createSessionModel);
            await sessionGrain.PauseSessionAsync();

            // Act
            var result = await sessionGrain.ResumeSessionAsync();

            // Assert
            var session = await sessionGrain.GetSessionAsync();
            result.IsSuccess.Should().BeTrue();
            session.IsSuccess.Should().BeTrue();
            session.Value.Status.Should().Be(SessionStatus.Active);
        }

        [Fact]
        public async Task ResumeSessionAsync_WhenSessionIsNotExists_ReturnFail()
        {
            // Arrange
            var sessionId = Guid.NewGuid().ToString();
            var sessionGrain = _testApp.Cluster.Client.GetGrain<ISessionGrain>(sessionId);

            // Act
            var result = await sessionGrain.ResumeSessionAsync();

            // Assert
            result.IsFailed.Should().BeTrue();
        }
        
        #endregion

        #region AddOrUpdateProperty

        [Fact]
        public async Task AddOrUpdateProperty_WhenSessionIsExists_ReturnSuccess()
        {
            // Arrange
            var sessionId = Guid.NewGuid().ToString();
            string newPropertyValue = "user data"; 
            var createSessionModel = GetTestCreateSessionModel(sessionId);
            var sessionGrain = _testApp.Cluster.Client.GetGrain<ISessionGrain>(sessionId);
            await sessionGrain.CreateAsync(createSessionModel);

            // Act
            var result = await sessionGrain.AddOrUpdateProperty(ClaimTypes.UserData, newPropertyValue);

            // Assert
            var claims = await sessionGrain.ValidateAndGetClaimsAsync();
            result.IsSuccess.Should().BeTrue();
            claims.IsSuccess.Should().BeTrue();
            claims.Value.Should().ContainKey(ClaimTypes.UserData);
            claims.Value.Should().ContainValue(newPropertyValue);
        }

        [Fact]
        public async Task AddOrUpdateProperty_WhenSessionIsNotExists_ReturnSuccess()
        {
            // Arrange
            var sessionId = Guid.NewGuid().ToString();
            var sessionGrain = _testApp.Cluster.Client.GetGrain<ISessionGrain>(sessionId);
            
            // Act
            var result = await sessionGrain.AddOrUpdateProperty(ClaimTypes.UserData, "value");

            // Assert
            result.IsFailed.Should().BeTrue();
        }

        [Fact]
        public async Task AddOrUpdateProperty_UpdateProperty_ReturnSuccess()
        {
            // Arrange
            var sessionId = Guid.NewGuid().ToString();
            string newPropertyValue = "test20230@gmail.com"; 
            var createSessionModel = GetTestCreateSessionModel(sessionId);
            var sessionGrain = _testApp.Cluster.Client.GetGrain<ISessionGrain>(sessionId);
            await sessionGrain.CreateAsync(createSessionModel);
            
            // Act
            var result = await sessionGrain.AddOrUpdateProperty(ClaimTypes.Email, newPropertyValue);

            // Assert
            var claims = await sessionGrain.ValidateAndGetClaimsAsync();
            result.IsSuccess.Should().BeTrue();
            claims.IsSuccess.Should().BeTrue();
            claims.Value.Should().ContainKey(ClaimTypes.Email);
            claims.Value.Should().ContainValue(newPropertyValue);
        }

        #endregion
    }
}
