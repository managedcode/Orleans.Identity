using ManagedCode.Orleans.Identity.Models;
using Orleans.Runtime;
using System.Security.Claims;

namespace ManagedCode.Orleans.Identity.Tests.Helpers
{
    public static class SessionHelper
    {
        public static Dictionary<string, string> SetTestClaims(string sessionId) =>
            new Dictionary<string, string>
            {
                { ClaimTypes.MobilePhone, "+380500000" },
                { ClaimTypes.Email, "test@gmail.com" },
                { ClaimTypes.Sid, sessionId },
                { ClaimTypes.Actor, Guid.NewGuid().ToString() },
                { ClaimTypes.Role, "admin" }
            };

        public static CreateSessionModel GetTestCreateSessionModel(string sessionId)
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
    }
}
