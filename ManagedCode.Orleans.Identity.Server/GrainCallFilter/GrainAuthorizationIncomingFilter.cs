using System;
using System.Collections.Concurrent;
using System.Reflection;
using System.Security.Claims;
using System.Threading.Tasks;
using ManagedCode.Orleans.Identity.Core.Constants;
using Microsoft.AspNetCore.Authorization;
using Orleans.Runtime;
using Orleans.Configuration;
using Orleans.Serialization.Invocation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace ManagedCode.Orleans.Identity.Server.GrainCallFilter;

public class GrainAuthorizationIncomingFilter(
    IAuthorizationService authorizationService,
    IAuthorizationPolicyProvider policyProvider) : IIncomingGrainCallFilter
{
    private const string AccessDeniedNotAuthenticated = "Access denied. User is not authenticated.";
    private const string AccessDeniedNotAuthorized = "Access denied. User is not authorized.";
    private const string UnsupportedAuthenticationSchemes =
        "AuthenticationSchemes is not supported by Orleans grain authorization.";

    private readonly ConcurrentDictionary<(MethodInfo Interface, MethodInfo Implementation, Type? Grain), AuthorizationMetadata?>
        authorizationMetadata = new();

    public async Task Invoke(IIncomingGrainCallContext context)
    {
        if (context.InterfaceMethod.DeclaringType == typeof(IAsyncEnumerableGrainExtension))
        {
            await InvokeStreamingAsync(context);
            return;
        }
        await RequireAuthorizationAsync(context);
        await context.Invoke();
    }

    private async Task RequireAuthorizationAsync(IIncomingGrainCallContext context)
    {
        var metadata = authorizationMetadata.GetOrAdd(
            (GetMethodDefinition(context.InterfaceMethod), GetMethodDefinition(context.ImplementationMethod), context.Grain?.GetType()),
            static key => GetAuthorizationMetadata(key.Interface, key.Implementation, key.Grain));

        if (metadata is not null)
        {
            var user = GetUserFromRequestContext();

            if (user == null || user.Identity?.IsAuthenticated != true)
            {
                throw new UnauthorizedAccessException(AccessDeniedNotAuthenticated);
            }

            if (metadata.HasAuthenticationSchemes)
            {
                throw new InvalidOperationException(UnsupportedAuthenticationSchemes);
            }

            await AuthorizeAsync(context, user, metadata);
        }

    }

    private async Task InvokeStreamingAsync(IIncomingGrainCallContext context)
    {
        if (context.Request.GetArgument(0) is not Guid id || id == Guid.Empty)
        {
            throw new UnauthorizedAccessException(AccessDeniedNotAuthorized);
        }
        StreamingAuthorizationState state;
        var principal = GetUserFromRequestContext();
        var methodName = context.InterfaceMethod.Name;
        var start = methodName == nameof(IAsyncEnumerableGrainExtension.StartEnumeration);
        var disposing = methodName == nameof(IAsyncEnumerableGrainExtension.DisposeAsync);
        StreamingAuthorizationState.Entry entry;
        if (start)
        {
            if (context.Request.GetArgument(1) is not IInvokable request)
            {
                throw new UnauthorizedAccessException(AccessDeniedNotAuthorized);
            }
            var operation = StreamingGrainCallContext.ResolveOperation(context.TargetContext, request);
            await RequireAuthorizationAsync(new StreamingGrainCallContext(context, operation));
            state = GetOrCreateStreamingState(context.TargetContext);
            entry = state.Add(id, operation, context.SourceId, principal);
        }
        else
        {
            var existingState = context.TargetContext.GetComponent<StreamingAuthorizationState>();
            if (existingState is null || existingState.Find(id) is not { } current)
            {
                if (disposing)
                {
                    // Keep disposal idempotent without touching an enumeration whose owner is unknown.
                    context.Response = Response.Completed;
                    return;
                }
                throw new UnauthorizedAccessException(AccessDeniedNotAuthorized);
            }
            state = existingState;
            entry = current;
            if (!entry.CanContinue(context.SourceId, principal, disposing))
            {
                throw new UnauthorizedAccessException(AccessDeniedNotAuthorized);
            }
            await RequireAuthorizationAsync(new StreamingGrainCallContext(context, entry.Operation));
            if (!state.TryBegin(id, entry, context.SourceId, principal, disposing))
            {
                throw new UnauthorizedAccessException(AccessDeniedNotAuthorized);
            }
        }
        var terminated = false;
        try
        {
            await context.Invoke();
            terminated = disposing || IsTerminalResult(context.Response);
        }
        catch
        {
            terminated = start;
            throw;
        }
        finally
        {
            if (entry.Finish(disposing, terminated))
            {
                state.Remove(id, entry);
            }
        }
    }

    private static bool IsTerminalResult(Response? response) =>
        response is Response<(EnumerationResult Status, object? Value)> enumeration &&
        enumeration.TypedResult.Status is EnumerationResult.Completed or EnumerationResult.CompletedWithElement or EnumerationResult.CompletedWithBatch
            or EnumerationResult.MissingEnumeratorError or EnumerationResult.Error or EnumerationResult.Canceled;

    private static StreamingAuthorizationState GetOrCreateStreamingState(IGrainContext target)
    {
        var state = target.GetComponent<StreamingAuthorizationState>();
        if (state is null)
        {
            var timeout = target.ActivationServices.GetRequiredService<IOptions<MessagingOptions>>().Value.ResponseTimeout;
            state = new StreamingAuthorizationState(timeout * 3);
            target.SetComponent(state);
        }
        return state;
    }

    private static ClaimsPrincipal? GetUserFromRequestContext()
    {
        var requestContext = RequestContext.Get(OrleansIdentityConstants.USER_CLAIMS);
        return requestContext as ClaimsPrincipal;
    }

    private static MethodInfo GetMethodDefinition(MethodInfo method) =>
        method.IsConstructedGenericMethod ? method.GetGenericMethodDefinition() : method;

    private static AuthorizationMetadata? GetAuthorizationMetadata(
        MethodInfo interfaceMethod, MethodInfo implementationMethod, Type? grainType)
    {
        if (HasAllowAnonymousAttribute(interfaceMethod) || HasAllowAnonymousAttribute(implementationMethod))
        {
            return null;
        }

        var methodAttributes = GetAuthorizeAttributes(interfaceMethod);
        var implementationAttributes = GetAuthorizeAttributes(implementationMethod);
        var interfaceType = interfaceMethod.DeclaringType;
        var implementationType = implementationMethod.DeclaringType;
        if (implementationType?.IsAssignableFrom(grainType) == true)
        {
            implementationType = grainType;
        }

        if (methodAttributes.Length == 0 && implementationAttributes.Length == 0
            && ((interfaceType is not null && HasAllowAnonymousAttribute(interfaceType))
                || (implementationType is not null && HasAllowAnonymousAttribute(implementationType))))
        {
            return null;
        }

        AuthorizeAttribute[] authorizeData = [
            .. interfaceType is null ? [] : GetAuthorizeAttributes(interfaceType),
            .. implementationType is null ? [] : GetAuthorizeAttributes(implementationType),
            .. methodAttributes,
            .. implementationAttributes,
        ];
        return authorizeData.Length == 0 ? null : new AuthorizationMetadata(authorizeData);
    }

    private static bool HasAllowAnonymousAttribute(MemberInfo memberInfo)
    {
        return Attribute.IsDefined(memberInfo, typeof(AllowAnonymousAttribute), inherit: true);
    }

    private static AuthorizeAttribute[] GetAuthorizeAttributes(MemberInfo memberInfo) =>
        (AuthorizeAttribute[])Attribute.GetCustomAttributes(memberInfo, typeof(AuthorizeAttribute), inherit: true);

    private async Task AuthorizeAsync(
        IIncomingGrainCallContext context,
        ClaimsPrincipal user,
        AuthorizationMetadata metadata)
    {
        var authorizationPolicy = metadata.CachedPolicy;
        if (authorizationPolicy is null)
        {
            authorizationPolicy = await AuthorizationPolicy.CombineAsync(policyProvider, metadata.AuthorizeData);
            if (policyProvider.AllowsCachingPolicies)
            {
                metadata.CachedPolicy = authorizationPolicy;
            }
        }

        if (authorizationPolicy is null)
        {
            return;
        }

        if (authorizationPolicy.AuthenticationSchemes.Count != 0)
        {
            throw new InvalidOperationException(UnsupportedAuthenticationSchemes);
        }

        var authorizationResult = await authorizationService.AuthorizeAsync(user, context, authorizationPolicy);

        if (!authorizationResult.Succeeded)
        {
            throw new UnauthorizedAccessException(AccessDeniedNotAuthorized);
        }
    }

    private sealed class AuthorizationMetadata(AuthorizeAttribute[] authorizeData)
    {
        public AuthorizeAttribute[] AuthorizeData { get; } = authorizeData;
        public bool HasAuthenticationSchemes { get; } = Array.Exists(authorizeData,
            static attribute => !string.IsNullOrWhiteSpace(attribute.AuthenticationSchemes));
        public volatile AuthorizationPolicy? CachedPolicy;
    }
}
