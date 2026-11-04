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
        var metadata = GetCachedAuthorizationMetadata(context.InterfaceMethod, context.ImplementationMethod, context.Grain?.GetType());
        if (metadata is not null)
        {
            await RequireAuthorizationAsync(context, metadata);
        }

        await context.Invoke();
    }

    private AuthorizationMetadata? GetCachedAuthorizationMetadata(MethodInfo interfaceMethod, MethodInfo implementationMethod, Type? grainType) =>
        authorizationMetadata.GetOrAdd(
            (GetMethodDefinition(interfaceMethod), GetMethodDefinition(implementationMethod), grainType),
            static key => GetAuthorizationMetadata(key.Interface, key.Implementation, key.Grain));

    private Task RequireStreamingAuthorizationAsync(IIncomingGrainCallContext context, StreamingGrainCallContext.Operation operation)
    {
        var metadata = GetCachedAuthorizationMetadata(operation.InterfaceMethod, operation.ImplementationMethod,
            context.TargetContext.GrainInstance?.GetType());
        return metadata is null
            ? Task.CompletedTask
            : RequireAuthorizationAsync(new StreamingGrainCallContext(context, operation), metadata);
    }

    private async Task RequireAuthorizationAsync(IIncomingGrainCallContext context, AuthorizationMetadata metadata)
    {
        var user = GetUserFromRequestContext();
        if (user?.Identity?.IsAuthenticated != true)
        {
            throw new UnauthorizedAccessException(AccessDeniedNotAuthenticated);
        }

        if (metadata.HasAuthenticationSchemes)
        {
            throw new InvalidOperationException(UnsupportedAuthenticationSchemes);
        }

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
            entry = new StreamingAuthorizationState.Entry(operation, context.SourceId, principal);
            await RequireStreamingAuthorizationAsync(context, operation);
            if (!entry.CanContinue(context.SourceId, GetUserFromRequestContext(), disposing: false))
            {
                throw new UnauthorizedAccessException(AccessDeniedNotAuthorized);
            }

            state = GetOrCreateStreamingState(context.TargetContext);
            state.Add(id, entry);
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
            await RequireStreamingAuthorizationAsync(context, entry.Operation);
            if (!state.TryBegin(id, entry, context.SourceId, principal, disposing))
            {
                throw new UnauthorizedAccessException(AccessDeniedNotAuthorized);
            }
        }
        var terminated = false;
        try
        {
            await context.Invoke();
            var response = context.Response;
            terminated = response is { Exception: not null }
                ? start
                : disposing || IsTerminalResult(response);
        }
        catch
        {
            terminated = start;
            throw;
        }
        finally
        {
            state.Finish(id, entry, disposing, terminated);
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
            var timeProvider = target.ActivationServices.GetRequiredService<TimeProvider>();
            state = new StreamingAuthorizationState(timeout * 3, timeProvider);
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
        var methodAttributes = GetAttributes(interfaceMethod);
        var implementationAttributes = GetAttributes(implementationMethod);
        if (AllowsAnonymous(methodAttributes) || AllowsAnonymous(implementationAttributes))
        {
            return null;
        }

        var methodAuthorizeData = methodAttributes.OfType<IAuthorizeData>().ToArray();
        var implementationAuthorizeData = implementationAttributes.OfType<IAuthorizeData>().ToArray();
        var interfaceType = interfaceMethod.DeclaringType;
        var implementationType = implementationMethod.DeclaringType;
        if (implementationType?.IsAssignableFrom(grainType) == true)
        {
            implementationType = grainType;
        }

        var interfaceAttributes = GetAttributes(interfaceType);
        var typeAttributes = GetAttributes(implementationType);
        if (methodAuthorizeData.Length == 0 && implementationAuthorizeData.Length == 0
            && (AllowsAnonymous(interfaceAttributes) || AllowsAnonymous(typeAttributes)))
        {
            return null;
        }

        IAuthorizeData[] authorizeData = [
            .. interfaceAttributes.OfType<IAuthorizeData>(),
            .. typeAttributes.OfType<IAuthorizeData>(),
            .. methodAuthorizeData,
            .. implementationAuthorizeData,
        ];
        return authorizeData.Length == 0 ? null : new AuthorizationMetadata(authorizeData);
    }

    private static bool AllowsAnonymous(Attribute[] attributes) =>
        Array.Exists(attributes, static attribute => attribute is IAllowAnonymous);

    private static Attribute[] GetAttributes(MemberInfo? memberInfo) =>
        memberInfo is null ? [] : Attribute.GetCustomAttributes(memberInfo, inherit: true);

    private sealed class AuthorizationMetadata(IAuthorizeData[] authorizeData)
    {
        public IAuthorizeData[] AuthorizeData { get; } = authorizeData;
        public bool HasAuthenticationSchemes { get; } = Array.Exists(authorizeData,
            static attribute => !string.IsNullOrWhiteSpace(attribute.AuthenticationSchemes));
        public volatile AuthorizationPolicy? CachedPolicy;
    }
}
