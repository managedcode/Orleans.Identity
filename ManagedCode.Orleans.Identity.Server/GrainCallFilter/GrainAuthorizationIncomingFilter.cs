using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Orleans;
using Orleans.Runtime;

namespace ManagedCode.Orleans.Identity.Server.GrainCallFilter;

public class GrainAuthorizationIncomingFilter : IIncomingGrainCallFilter
{
    public async Task Invoke(IIncomingGrainCallContext context)
    {
        if (IsGrainAuthorized(context.ImplementationMethod, out var attributes))
        {
            var user = GetUserFromRequestContext();
            var isAuthorized = false;
            
            if (user?.Identity?.IsAuthenticated == true)
            {
                if (attributes.All(attribute => string.IsNullOrWhiteSpace(attribute.Roles)))
                {
                    isAuthorized = true;
                }
                else
                {
                    var userRoles = user.FindAll(ClaimTypes.Role).Select(c => c.Value).ToHashSet();
                    
                    foreach (var attribute in attributes)
                    {
                        var requiredRoles = attribute.Roles?.Split(',', StringSplitOptions.RemoveEmptyEntries) ?? Array.Empty<string>();
                        if (requiredRoles.Any(role => userRoles.Contains(role.Trim())))
                        {
                            isAuthorized = true;
                            break;
                        }
                    }
                }
            }
            
            if (!isAuthorized)
            {
                // Set authorization failure flag instead of throwing exception
                RequestContext.Set("AuthorizationFailed", true);
                RequestContext.Set("AuthorizationMessage", "Access denied. User is not authenticated or does not have required roles.");
            }
        }

        await context.Invoke();
    }

    private static ClaimsPrincipal? GetUserFromRequestContext()
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

        return null;
    }

    private static bool IsGrainAuthorized(MemberInfo methodInfo, out List<AuthorizeAttribute> attributes)
    {
        attributes = new List<AuthorizeAttribute>();

        // Check for AllowAnonymous on method
        if (Attribute.IsDefined(methodInfo, typeof(AllowAnonymousAttribute)))
        {
            return false;
        }

        // Check for Authorize on class
        if (methodInfo.DeclaringType != null && Attribute.IsDefined(methodInfo.DeclaringType, typeof(AuthorizeAttribute)))
        {
            attributes.AddRange(Attribute.GetCustomAttributes(methodInfo.DeclaringType, typeof(AuthorizeAttribute))
                .Cast<AuthorizeAttribute>());
        }

        // Check for Authorize on method
        if (Attribute.IsDefined(methodInfo, typeof(AuthorizeAttribute)))
        {
            attributes.AddRange(Attribute.GetCustomAttributes(methodInfo, typeof(AuthorizeAttribute))
                .Cast<AuthorizeAttribute>());
            return true;
        }

        return attributes.Any();
    }
}