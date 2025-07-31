using System.Security.Claims;

namespace ManagedCode.Orleans.Identity.Tests.Helpers;

public static class SessionHelper
{
    public static Dictionary<string, HashSet<string>> SetTestClaims(string sessionId)
    {
        return new()
        {
            { ClaimTypes.MobilePhone, new HashSet<string> { "+380500000" } },
            { ClaimTypes.Email, new HashSet<string> { "test@gmail.com" } },
            { ClaimTypes.Sid, new HashSet<string> { sessionId } },
            { ClaimTypes.Actor, new HashSet<string> { Guid.NewGuid().ToString() } },
            { ClaimTypes.Role, new HashSet<string> { "admin" } }
        };
    }

    public static GrainId GetTestUserGrainId()
    {
        var userId = Guid.NewGuid().ToString();
        return GrainId.Create("UserGrain", userId);
    }
}