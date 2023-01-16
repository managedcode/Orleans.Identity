using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using ManagedCode.Orleans.Identity.Middlewares;
using Microsoft.AspNetCore.Authorization;
using Orleans;

namespace ManagedCode.Orleans.Identity.GrainCallFilter;

public class GrainAuthorizationIncomingFilter : IIncomingGrainCallFilter
{
    private readonly IClusterClient _client;

    public GrainAuthorizationIncomingFilter(IClusterClient client)
    {
        _client = client;
    }

    public async Task Invoke(IIncomingGrainCallContext context)
    {
        if (IsAuthorize(context.ImplementationMethod, out var attributes))
        {
            var roles = this.GetOrleansContext().ToHashSet();
            foreach (var attribute in attributes)
            {
                var intersect = attribute.Roles?.Split(',') ?? Array.Empty<string>();
                if (intersect.Any(role => roles.Contains(role)))
                {
                    await context.Invoke();
                    return;
                }

                throw new UnauthorizedAccessException();
            }
        }
        else
        {
            await context.Invoke();
        }
    }

    private static bool IsAuthorize(MemberInfo methodInfo, out List<AuthorizeAttribute> attributes)
    {
        attributes = new List<AuthorizeAttribute>();

        // for method
        if (Attribute.IsDefined(methodInfo, typeof(AllowAnonymousAttribute)))
        {
            return false;
        }

        if (methodInfo.DeclaringType != null && Attribute.IsDefined(methodInfo.DeclaringType, typeof(AuthorizeAttribute)))
        {
            attributes.AddRange(Attribute.GetCustomAttributes(methodInfo.DeclaringType, typeof(AuthorizeAttribute)).Select(s => (AuthorizeAttribute)s));
        }

        if (Attribute.IsDefined(methodInfo, typeof(AuthorizeAttribute)))
        {
            attributes.AddRange(Attribute.GetCustomAttributes(methodInfo, typeof(AuthorizeAttribute)).Select(s => (AuthorizeAttribute)s));
            return true;
        }

        return attributes.Any();
    }
}