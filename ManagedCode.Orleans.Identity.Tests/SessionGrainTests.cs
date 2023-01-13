using FluentAssertions;
using ManagedCode.Orleans.Identity.Grains.Interfaces;
using ManagedCode.Orleans.Identity.Models;
using ManagedCode.Orleans.Identity.Shared.Enums;
using ManagedCode.Orleans.Identity.Tests.Cluster;
using ManagedCode.Orleans.Identity.Tests.Helpers;
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

        private CreateSessionModel GetTestCreateSessionModel(string sessionId)
        {
            string userId = Guid.NewGuid().ToString();

            GrainId userGrainId = GrainId.Create("UserGrain", userId);
            
            CreateSessionModel createSessionModel = new CreateSessionModel
            {
                UserData = SessionHelper.SetTestClaims(sessionId),
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

        #region RemoveProperty

        [Fact]
        public async Task RemoveProperty_WhenPropertyExists_ReturnSuccess()
        {
            // Arrange
            var sessionId = Guid.NewGuid().ToString();
            var createSessionModel = GetTestCreateSessionModel(sessionId);
            var sessionGrain = _testApp.Cluster.Client.GetGrain<ISessionGrain>(sessionId);
            await sessionGrain.CreateAsync(createSessionModel);
            
            // Act
            var result = await sessionGrain.RemoveProperty(ClaimTypes.MobilePhone);

            // Assert
            var claims = await sessionGrain.ValidateAndGetClaimsAsync();
            result.IsSuccess.Should().BeTrue();
            claims.IsSuccess.Should().BeTrue();
            claims.Value.Should().NotContainKey(ClaimTypes.MobilePhone);
        }

        [Fact]
        public async Task RemoveProperty_WhenPropertyIsNotExists_ReturnFail()
        {
            // Arrange
            var sessionId = Guid.NewGuid().ToString();
            var createSessionModel = GetTestCreateSessionModel(sessionId);
            var sessionGrain = _testApp.Cluster.Client.GetGrain<ISessionGrain>(sessionId);
            await sessionGrain.CreateAsync(createSessionModel);

            // Act
            var result = await sessionGrain.RemoveProperty(ClaimTypes.WindowsDeviceGroup);

            // Assert
            result.IsFailed.Should().BeTrue();

        }

        [Fact]
        public async Task RemoveProperty_WhenSessionIsNotExists_ReturnFail()
        {
            // Arrange
            var sessionId = Guid.NewGuid().ToString();
            var sessionGrain = _testApp.Cluster.Client.GetGrain<ISessionGrain>(sessionId);

            // Act
            var result = await sessionGrain.RemoveProperty(ClaimTypes.MobilePhone);
            
            // Assert
            result.IsFailed.Should().BeTrue();
        }

        #endregion

        #region AddProperty

        [Fact]
        public async Task AddProperty_WhenSessionExist_ReturnSuccess()
        {
            // Arrange
            var sessionId = Guid.NewGuid().ToString();
            string propertyValue = "Name";
            var sessionCreateModel = SessionHelper.GetTestCreateSessionModel(sessionId);
            var sessionGrain = _testApp.Cluster.Client.GetGrain<ISessionGrain>(sessionId);
            await sessionGrain.CreateAsync(sessionCreateModel);

            // Act
            var result = await sessionGrain.AddProperty(ClaimTypes.Name, propertyValue);

            // Assert
            var userData = await sessionGrain.ValidateAndGetClaimsAsync();
            result.IsSuccess.Should().BeTrue();
            userData.IsSuccess.Should().BeTrue();
            userData.Value.Should().ContainKey(ClaimTypes.Name).WhoseValue.Should().BeEquivalentTo(propertyValue);
        }

        [Fact]
        public async Task AddProperty_WhenSessionNotExist_ReturnFail()
        {
            // Arrange
            var sessionId = Guid.NewGuid().ToString();
            string propertyValue = "Name";
            var sessionGrain = _testApp.Cluster.Client.GetGrain<ISessionGrain>(sessionId);

            // Act
            var result = await sessionGrain.AddProperty(ClaimTypes.Name, propertyValue);

            // Assert
            result.IsFailed.Should().BeTrue();
        }

        [Fact]
        public async Task AddProperty_WhenPropertyAlreadyExist_ReturnFail()
        {
            // Arrange
            var sessionId = Guid.NewGuid().ToString();
            string propertyValue = "Name";
            var sessionCreateModel = SessionHelper.GetTestCreateSessionModel(sessionId);
            var sessionGrain = _testApp.Cluster.Client.GetGrain<ISessionGrain>(sessionId);
            await sessionGrain.CreateAsync(sessionCreateModel);

            // Act
            var result = await sessionGrain.AddProperty(ClaimTypes.Email, propertyValue);

            // Assert
            result.IsFailed.Should().BeTrue();
        }

        [Fact]
        public async Task AddProperty_WithValues_WhenSessionExist_ReturnSuccess()
        {
            // Arrange
            var sessionId = Guid.NewGuid().ToString();
            var propertyValues = new List<string> { "one", "two" };
            var sessionCreateModel = SessionHelper.GetTestCreateSessionModel(sessionId);
            var sessionGrain = _testApp.Cluster.Client.GetGrain<ISessionGrain>(sessionId);
            await sessionGrain.CreateAsync(sessionCreateModel);

            // Act
            var result = await sessionGrain.AddProperty(ClaimTypes.System, propertyValues);

            // Assert
            var userData = await sessionGrain.ValidateAndGetClaimsAsync();
            result.IsSuccess.Should().BeTrue();
            userData.IsSuccess.Should().BeTrue();
            userData.Value.Should().ContainKey(ClaimTypes.System).WhoseValue.Should().BeEquivalentTo(propertyValues);
        }

        [Fact]
        public async Task AddProperty_WithValuesThatHaveDuplicates_WhenSessionExist_ReturnSuccess()
        {
            // Arrange
            var sessionId = Guid.NewGuid().ToString();
            var propertyValues = new List<string> { "one", "two", "two" };
            var expectedpropertyValues = new List<string> { "one", "two" };
            var sessionCreateModel = SessionHelper.GetTestCreateSessionModel(sessionId);
            var sessionGrain = _testApp.Cluster.Client.GetGrain<ISessionGrain>(sessionId);
            await sessionGrain.CreateAsync(sessionCreateModel);

            // Act
            var result = await sessionGrain.AddProperty(ClaimTypes.System, propertyValues);

            // Assert
            var userData = await sessionGrain.ValidateAndGetClaimsAsync();
            result.IsSuccess.Should().BeTrue();
            userData.IsSuccess.Should().BeTrue();
            userData.Value.Should().ContainKey(ClaimTypes.System).WhoseValue.Should().BeEquivalentTo(expectedpropertyValues);
        }

        #endregion

        #region ReplaceProperty

        [Fact]
        public async Task ReplaceProperty_WhenPropertyExists_ReturnSuccess()
        {
            // Arrange
            var sessionId = Guid.NewGuid().ToString();
            string newPropertyValue = "test22@gmail.com";
            var sessionCreateModel = SessionHelper.GetTestCreateSessionModel(sessionId);
            var sessionGrain = _testApp.Cluster.Client.GetGrain<ISessionGrain>(sessionId);
            await sessionGrain.CreateAsync(sessionCreateModel);

            // Act
            var result = await sessionGrain.ReplaceProperty(ClaimTypes.Email, newPropertyValue);

            // Assert
            var userData = await sessionGrain.ValidateAndGetClaimsAsync();
            result.IsSuccess.Should().BeTrue();
            userData.IsSuccess.Should().BeTrue();
            userData.Value.Should().ContainKey(ClaimTypes.Email).WhoseValue.Should().BeEquivalentTo(newPropertyValue);
        }

        [Fact]
        public async Task ReplaceProperty_WhenPropertyNotExist_ReturnFail()
        {
            // Arrange
            var sessionId = Guid.NewGuid().ToString();
            string newPropertyValue = "new name";
            var sessionCreateModel = SessionHelper.GetTestCreateSessionModel(sessionId);
            var sessionGrain = _testApp.Cluster.Client.GetGrain<ISessionGrain>(sessionId);
            await sessionGrain.CreateAsync(sessionCreateModel);

            // Act
            var result = await sessionGrain.ReplaceProperty(ClaimTypes.Name, newPropertyValue);

            // Assert
            result.IsFailed.Should().BeTrue();
        }

        [Fact]
        public async Task ReplaceProperty_WhenSessionNotExists_ReturnFail()
        {
            // Arrange
            var sessionId = Guid.NewGuid().ToString();
            string newPropertyValue = "test22@gmail.com";
            var sessionGrain = _testApp.Cluster.Client.GetGrain<ISessionGrain>(sessionId);

            // Act
            var result = await sessionGrain.ReplaceProperty(ClaimTypes.Email, newPropertyValue);

            // Assert
            result.IsFailed.Should().BeTrue();
        }

        [Fact]
        public async Task ReplaceProperty_WithValues_WhenPropertyExists_ReturnSuccess()
        {
            // Arrange
            var sessionId = Guid.NewGuid().ToString();
            var newPropertyValues = new List<string> { "admin", "moderator" };
            var sessionCreateModel = SessionHelper.GetTestCreateSessionModel(sessionId);
            var sessionGrain = _testApp.Cluster.Client.GetGrain<ISessionGrain>(sessionId);
            await sessionGrain.CreateAsync(sessionCreateModel);

            // Act
            var result = await sessionGrain.ReplaceProperty(ClaimTypes.Role, newPropertyValues);

            // Assert
            var userData = await sessionGrain.ValidateAndGetClaimsAsync();
            result.IsSuccess.Should().BeTrue();
            userData.IsSuccess.Should().BeTrue();
            userData.Value.Should().ContainKey(ClaimTypes.Role).WhoseValue.Should().BeEquivalentTo(newPropertyValues);
        }

        [Fact]
        public async Task ReplaceProperty_WithValuesThatHaveDuplicates_WhenPropertyExists_ReturnSuccess()
        {
            // Arrange
            var sessionId = Guid.NewGuid().ToString();
            var newPropertyValues = new List<string> { "admin", "moderator", "moderator" };
            var expectedPropertyValues = new List<string> { "admin", "moderator" };
            var sessionCreateModel = SessionHelper.GetTestCreateSessionModel(sessionId);
            var sessionGrain = _testApp.Cluster.Client.GetGrain<ISessionGrain>(sessionId);
            await sessionGrain.CreateAsync(sessionCreateModel);

            // Act
            var result = await sessionGrain.ReplaceProperty(ClaimTypes.Role, newPropertyValues);

            // Assert
            var userData = await sessionGrain.ValidateAndGetClaimsAsync();
            result.IsSuccess.Should().BeTrue();
            userData.IsSuccess.Should().BeTrue();
            userData.Value.Should().ContainKey(ClaimTypes.Role).WhoseValue.Should().BeEquivalentTo(expectedPropertyValues);
        }

        #endregion

        #region RemoveValueFromProperty

        #endregion
    }
}
