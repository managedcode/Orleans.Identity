using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Claims;
using System.Threading.Tasks;
using ManagedCode.Orleans.Identity.Core.Constants;
using Microsoft.AspNetCore.Authorization;
using Orleans.Runtime;

namespace ManagedCode.Orleans.Identity.Server.GrainCallFilter;

public class GrainAuthorizationIncomingFilter : IIncomingGrainCallFilter
{
    private const int EmptyAttributeCount = 0;
    private const char RoleSeparator = ',';
    private const string AccessDeniedNotAuthenticated = "Access denied. User is not authenticated.";
    private const string AccessDeniedMissingRoles = "Access denied. User does not have required roles.";

    public async Task Invoke(IIncomingGrainCallContext context)
    {
        if (IsGrainAuthorized(context, out var attributes))
        {
            var user = GetUserFromRequestContext();

            if (user == null || user.Identity?.IsAuthenticated != true)
            {
                throw new UnauthorizedAccessException(AccessDeniedNotAuthenticated);
            }

            if (!HasRequiredRoles(attributes, user))
            {
                throw new UnauthorizedAccessException(AccessDeniedMissingRoles);
            }
        }

        await context.Invoke();
    }

    private static ClaimsPrincipal? GetUserFromRequestContext()
    {
        var requestContext = RequestContext.Get(OrleansIdentityConstants.USER_CLAIMS);
        return requestContext as ClaimsPrincipal;
    }

    private static bool IsGrainAuthorized(IIncomingGrainCallContext context, out List<AuthorizeAttribute> attributes)
    {
        attributes = [];
        var members = GetAuthorizationMembers(context);

        if (members.Any(HasAllowAnonymousAttribute))
        {
            return false;
        }

        attributes.AddRange(members.SelectMany(GetAuthorizeAttributes));
        return attributes.Count != EmptyAttributeCount;
    }

    private static IReadOnlyList<MemberInfo> GetAuthorizationMembers(IIncomingGrainCallContext context)
    {
        var members = new List<MemberInfo>();

        if (context.InterfaceMethod.DeclaringType is { } interfaceType)
        {
            members.Add(interfaceType);
        }

        if (context.ImplementationMethod.DeclaringType is { } implementationType)
        {
            members.Add(implementationType);
        }

        members.Add(context.InterfaceMethod);
        members.Add(context.ImplementationMethod);

        return members;
    }

    private static bool HasAllowAnonymousAttribute(MemberInfo memberInfo)
    {
        return Attribute.IsDefined(memberInfo, typeof(AllowAnonymousAttribute), inherit: true);
    }

    private static IEnumerable<AuthorizeAttribute> GetAuthorizeAttributes(MemberInfo memberInfo)
    {
        return Attribute
            .GetCustomAttributes(memberInfo, typeof(AuthorizeAttribute), inherit: true)
            .Cast<AuthorizeAttribute>();
    }

    private static bool HasRequiredRoles(IEnumerable<AuthorizeAttribute> attributes, ClaimsPrincipal user)
    {
        return attributes
            .Where(attribute => !string.IsNullOrWhiteSpace(attribute.Roles))
            .All(attribute => HasAnyRequiredRole(attribute, user));
    }

    private static bool HasAnyRequiredRole(AuthorizeAttribute attribute, ClaimsPrincipal user)
    {
        if (string.IsNullOrWhiteSpace(attribute.Roles))
        {
            return true;
        }

        var roles = attribute.Roles.Split(
            RoleSeparator,
            StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries
        );

        return roles.Any(user.IsInRole);
    }
}
