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

public class GrainAuthorizationIncomingFilter(
    IAuthorizationService authorizationService,
    IAuthorizationPolicyProvider policyProvider) : IIncomingGrainCallFilter
{
    private const int EmptyAttributeCount = 0;
    private const string AccessDeniedNotAuthenticated = "Access denied. User is not authenticated.";
    private const string AccessDeniedNotAuthorized = "Access denied. User is not authorized.";
    private const string UnsupportedAuthenticationSchemes =
        "AuthorizeAttribute.AuthenticationSchemes is not supported by Orleans grain authorization.";

    public async Task Invoke(IIncomingGrainCallContext context)
    {
        if (IsGrainAuthorized(context, out var authorizeData))
        {
            var user = GetUserFromRequestContext();

            if (user == null || user.Identity?.IsAuthenticated != true)
            {
                throw new UnauthorizedAccessException(AccessDeniedNotAuthenticated);
            }

            ThrowIfUnsupportedAuthenticationSchemes(authorizeData);
            await AuthorizeAsync(context, user, authorizeData);
        }

        await context.Invoke();
    }

    private static ClaimsPrincipal? GetUserFromRequestContext()
    {
        var requestContext = RequestContext.Get(OrleansIdentityConstants.USER_CLAIMS);
        return requestContext as ClaimsPrincipal;
    }

    private static bool IsGrainAuthorized(IIncomingGrainCallContext context, out List<AuthorizeAttribute> authorizeData)
    {
        authorizeData = [];
        var members = GetAuthorizationMembers(context);

        if (members.Any(HasAllowAnonymousAttribute))
        {
            return false;
        }

        authorizeData.AddRange(members.SelectMany(GetAuthorizeAttributes));
        return authorizeData.Count != EmptyAttributeCount;
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

    private static void ThrowIfUnsupportedAuthenticationSchemes(IEnumerable<AuthorizeAttribute> authorizeData)
    {
        if (authorizeData.Any(attribute => !string.IsNullOrWhiteSpace(attribute.AuthenticationSchemes)))
        {
            throw new InvalidOperationException(UnsupportedAuthenticationSchemes);
        }
    }

    private async Task AuthorizeAsync(
        IIncomingGrainCallContext context,
        ClaimsPrincipal user,
        IEnumerable<IAuthorizeData> authorizeData)
    {
        var authorizationPolicy = await AuthorizationPolicy.CombineAsync(policyProvider, authorizeData);
        if (authorizationPolicy is null)
        {
            return;
        }

        var authorizationResult = await authorizationService.AuthorizeAsync(user, context, authorizationPolicy);

        if (!authorizationResult.Succeeded)
        {
            throw new UnauthorizedAccessException(AccessDeniedNotAuthorized);
        }
    }
}
