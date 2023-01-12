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

        public static CreateSessionModel GetTestCreateSessionModel(string sessionId, Dictionary<string, string> claims = null, bool replaceClaims = false)
        {
            string userId = Guid.NewGuid().ToString();

            GrainId userGrainId = GrainId.Create("UserGrain", userId);
            var userClaims = replaceClaims ? claims : SetTestClaims(sessionId);

            if (claims != null && replaceClaims is false)
            {
                foreach (var claim in claims)
                {
                    userClaims.TryAdd(claim.Key, claim.Value);
                }
            }
            
            CreateSessionModel createSessionModel = new CreateSessionModel
            {
                UserData = userClaims,
                UserGrainId = userGrainId
            };

            return createSessionModel;
        }
    }
}
