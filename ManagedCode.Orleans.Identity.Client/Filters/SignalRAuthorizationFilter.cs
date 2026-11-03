using System.Collections.Concurrent;
using System.Reflection;
using System.Security.Claims;
using Microsoft.AspNetCore.SignalR;

namespace ManagedCode.Orleans.Identity.Client.Filters;

public sealed class SignalRAuthorizationFilter : IHubFilter
{
    private static readonly ConcurrentDictionary<Type, Func<object, ClaimsPrincipal?, object>?> StreamWrappers = new();
    private static readonly MethodInfo WrapStreamMethod = typeof(SignalRAuthorizationFilter)
        .GetMethod(nameof(WrapStream), BindingFlags.NonPublic | BindingFlags.Static)!;

    public async ValueTask<object?> InvokeMethodAsync(
        HubInvocationContext invocationContext,
        Func<HubInvocationContext, ValueTask<object?>> next)
    {
        var user = invocationContext.Context.User;
        using var requestContextScope = new OrleansRequestContextScope(user);
        var result = await next(invocationContext);
        if (result is null)
        {
            return null;
        }

        var wrap = StreamWrappers.GetOrAdd(invocationContext.HubMethod.ReturnType, CreateStreamWrapper);
        return wrap is null ? result : wrap(result, user);
    }

    private static Func<object, ClaimsPrincipal?, object>? CreateStreamWrapper(Type returnType)
    {
        if (returnType.IsConstructedGenericType)
        {
            var definition = returnType.GetGenericTypeDefinition();
            if (definition == typeof(Task<>) || definition == typeof(ValueTask<>))
            {
                returnType = returnType.GenericTypeArguments[0];
            }
        }

        var stream = IsAsyncEnumerable(returnType) ? returnType : Array.Find(returnType.GetInterfaces(), IsAsyncEnumerable);
        return stream is null ? null : WrapStreamMethod
            .MakeGenericMethod(stream.GenericTypeArguments[0])
            .CreateDelegate<Func<object, ClaimsPrincipal?, object>>();
    }

    private static bool IsAsyncEnumerable(Type type) =>
        type.IsConstructedGenericType && type.GetGenericTypeDefinition() == typeof(IAsyncEnumerable<>);

    private static object WrapStream<T>(object stream, ClaimsPrincipal? user) =>
        new ScopedStream<T>((IAsyncEnumerable<T>)stream, user);

    private sealed class ScopedStream<T>(IAsyncEnumerable<T> stream, ClaimsPrincipal? user) : IAsyncEnumerable<T>
    {
        public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default)
        {
            using var scope = new OrleansRequestContextScope(user);
            return new ScopedEnumerator(stream.GetAsyncEnumerator(cancellationToken), user);
        }

        private sealed class ScopedEnumerator(IAsyncEnumerator<T> enumerator, ClaimsPrincipal? user) : IAsyncEnumerator<T>
        {
            public T Current
            {
                get
                {
                    using var scope = new OrleansRequestContextScope(user);
                    return enumerator.Current;
                }
            }

            public async ValueTask<bool> MoveNextAsync()
            {
                using var scope = new OrleansRequestContextScope(user);
                return await enumerator.MoveNextAsync();
            }

            public async ValueTask DisposeAsync()
            {
                using var scope = new OrleansRequestContextScope(user);
                await enumerator.DisposeAsync();
            }
        }
    }

    public async Task OnConnectedAsync(HubLifetimeContext context, Func<HubLifetimeContext, Task> next)
    {
        using var scope = new OrleansRequestContextScope(context.Context.User);
        await next(context);
    }

    public async Task OnDisconnectedAsync(
        HubLifetimeContext context, Exception? exception, Func<HubLifetimeContext, Exception?, Task> next)
    {
        using var scope = new OrleansRequestContextScope(context.Context.User);
        await next(context, exception);
    }
}
