using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using Orleans;
using Orleans.Runtime;
using ManagedCode.Orleans.Identity.Core.Constants;

namespace ManagedCode.Orleans.Identity.Server.GrainCallFilter;

public class GrainAuthorizationIncomingFilter : IIncomingGrainCallFilter
{
    public async Task Invoke(IIncomingGrainCallContext context)
    {
        // Check both interface method and implementation method
        if (IsGrainAuthorized(context.ImplementationMethod, out var attributes))
        {
            var user = GetUserFromRequestContext();
            
            if (user == null || user.Identity?.IsAuthenticated != true)
            {
                throw new UnauthorizedAccessException("Access denied. User is not authenticated.");
            }

            // Check if any attribute requires specific roles
            var rolesRequired = attributes.Any(attr => !string.IsNullOrWhiteSpace(attr.Roles));
            
            if (rolesRequired)
            {
                var userRoles = user.FindAll(ClaimTypes.Role).Select(c => c.Value).ToHashSet();
                
                // Check if user has any of the required roles from any attribute
                var hasRequiredRole = attributes.Any(attribute =>
                {
                    if (string.IsNullOrWhiteSpace(attribute.Roles))
                        return true; // No specific role required by this attribute
                    
                    var requiredRoles = attribute.Roles.Split(',', StringSplitOptions.RemoveEmptyEntries)
                        .Select(r => r.Trim());
                    
                    return requiredRoles.Any(role => userRoles.Contains(role));
                });

                if (!hasRequiredRole)
                {
                    throw new UnauthorizedAccessException("Access denied. User does not have required roles.");
                }
            }
        }
        
        await context.Invoke();
    }

    private static ClaimsPrincipal? GetUserFromRequestContext()
    {
        var requestContext = RequestContext.Get(OrleansIdentityConstants.USER_CLAIMS);
        return requestContext as ClaimsPrincipal;
    }

    private static bool IsGrainAuthorized(MemberInfo methodInfo, out List<AuthorizeAttribute> attributes)
    {
        attributes = [];

        if (Attribute.IsDefined(methodInfo, typeof(AllowAnonymousAttribute)))
        {
            return false;
        }

        if (methodInfo.DeclaringType != null && Attribute.IsDefined(methodInfo.DeclaringType, typeof(AuthorizeAttribute)))
        {
            attributes.AddRange(Attribute.GetCustomAttributes(methodInfo.DeclaringType, typeof(AuthorizeAttribute))
                .Cast<AuthorizeAttribute>());
        }

        if (Attribute.IsDefined(methodInfo, typeof(AuthorizeAttribute)))
        {
            attributes.AddRange(Attribute.GetCustomAttributes(methodInfo, typeof(AuthorizeAttribute))
                .Cast<AuthorizeAttribute>());
            return true;
        }

        return attributes.Any();
    }
}