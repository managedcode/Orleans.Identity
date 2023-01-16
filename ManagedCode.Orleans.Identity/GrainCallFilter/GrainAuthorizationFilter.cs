using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Orleans;
using System.Threading.Tasks;
using ManagedCode.Orleans.Identity.Shared.Constants;
using Microsoft.AspNetCore.Authorization;
using Orleans.Runtime;

namespace ManagedCode.Orleans.Identity.GrainCallFilter
{
    // Test filter
    // TODO: Get sessionID from request context and check if session valid
    public class GrainAuthorizationFilter : IIncomingGrainCallFilter
    {
        private readonly IGrainFactory _grainFactory;

        public GrainAuthorizationFilter(IGrainFactory grainFactory)
        {
            _grainFactory = grainFactory;
        }

        public Task Invoke(IIncomingGrainCallContext context)
        {
            IsAuthorized();
            return Task.FromResult(0);
        }

        private Task<bool> IsAuthorized()
        {
            var sessionId = RequestContext.Get(OrleansIdentityConstants.SESSION_ID_CLAIM_NAME);
            string session;
            if (sessionId != null) 
                session = sessionId.ToString();
            return Task.FromResult(true);
        }

        private static bool IsGrainAuthorized(MemberInfo methodInfo, out List<AuthorizeAttribute> attributes)
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
}
