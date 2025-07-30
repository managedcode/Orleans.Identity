using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using Orleans;
using Orleans.Runtime;

namespace ManagedCode.Orleans.Identity.Core.Extensions;

public static class GrainExtensions
{
    public static ClaimsPrincipal GetCurrentUser(this Grain grain)
    {
        var requestContext = RequestContext.Get("UserClaims");
        if (requestContext is Dictionary<string, string> claimsDict)
        {
            var claims = new List<Claim>();
            foreach (var kvp in claimsDict)
            {
                // Handle comma-separated values (like roles)
                var values = kvp.Value.Split(',', StringSplitOptions.RemoveEmptyEntries);
                foreach (var value in values)
                {
                    claims.Add(new Claim(kvp.Key, value.Trim()));
                }
            }
            var identity = new ClaimsIdentity(claims, "JWT");
            return new ClaimsPrincipal(identity);
        }

        return new ClaimsPrincipal(new ClaimsIdentity());
    }

    public static bool IsAuthorizationFailed(this Grain grain)
    {
        return RequestContext.Get("AuthorizationFailed") is bool failed && failed;
    }

    public static string? GetAuthorizationMessage(this Grain grain)
    {
        return RequestContext.Get("AuthorizationMessage") as string;
    }
} 