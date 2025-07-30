using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Orleans;
using Orleans.Runtime;
using ManagedCode.Orleans.Identity.Core.Constants;

namespace ManagedCode.Orleans.Identity.Server.GrainCallFilter;

public class GrainAuthorizationIncomingFilter : IIncomingGrainCallFilter
{
    public async Task Invoke(IIncomingGrainCallContext context)
    {
        if (IsGrainAuthorized(context.ImplementationMethod, out var attributes))
        {
            var user = GetUserFromRequestContext();

            if (user == null)
            {
                throw new UnauthorizedAccessException("Access denied. User is missing.");
            }

            if (user.Identity?.IsAuthenticated == false)
            {
                throw new UnauthorizedAccessException(
                    "Access denied. User is not authenticated or does not have required roles.");
            }


            var userRoles = user.FindAll(ClaimTypes.Role).Select(c => c.Value).ToHashSet();

            if (!(attributes.All(attribute => string.IsNullOrWhiteSpace(attribute.Roles)) ||
                  attributes.Any(attribute =>
                  {
                      var requiredRoles = attribute.Roles?.Split(',', StringSplitOptions.RemoveEmptyEntries) ?? [];
                      return requiredRoles.Any(role => userRoles.Contains(role.Trim()));
                  })))
            {
                throw new UnauthorizedAccessException(
                    "Access denied. User is not authenticated or does not have required roles.");
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

        if (methodInfo.DeclaringType != null &&
            Attribute.IsDefined(methodInfo.DeclaringType, typeof(AuthorizeAttribute)))
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