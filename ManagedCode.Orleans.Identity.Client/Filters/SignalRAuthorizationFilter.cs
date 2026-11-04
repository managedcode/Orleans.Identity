using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Security.Claims;
using System.Threading.Channels;
using Microsoft.AspNetCore.SignalR;

namespace ManagedCode.Orleans.Identity.Client.Filters;

public sealed class SignalRAuthorizationFilter : IHubFilter
{
    private static readonly ConditionalWeakTable<Type, Func<object, ClaimsPrincipal?, object>?> StreamWrappers = new();
    private static readonly MethodInfo WrapStreamMethod = typeof(SignalRAuthorizationFilter)
        .GetMethod(nameof(WrapStream), BindingFlags.NonPublic | BindingFlags.Static)!;
    private static readonly MethodInfo WrapChannelMethod = typeof(SignalRAuthorizationFilter)
        .GetMethod(nameof(WrapChannel), BindingFlags.NonPublic | BindingFlags.Static)!;

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

        var wrap = StreamWrappers.GetValue(invocationContext.HubMethod.ReturnType, CreateStreamWrapper);
        return wrap is null ? result : wrap(result, user);
    }

    private static Func<object, ClaimsPrincipal?, object>? CreateStreamWrapper(Type returnType)
    {
        returnType = GetAwaitedResultType(returnType) ?? returnType;

        var stream = IsAsyncEnumerable(returnType) ? returnType : Array.Find(returnType.GetInterfaces(), IsAsyncEnumerable);
        if (stream is not null)
        {
            return WrapStreamMethod.MakeGenericMethod(stream.GenericTypeArguments[0])
                .CreateDelegate<Func<object, ClaimsPrincipal?, object>>();
        }

        for (Type? type = returnType; type is not null; type = type.BaseType)
        {
            if (type.IsConstructedGenericType && type.GetGenericTypeDefinition() == typeof(ChannelReader<>))
            {
                return WrapChannelMethod.MakeGenericMethod(type.GenericTypeArguments[0])
                    .CreateDelegate<Func<object, ClaimsPrincipal?, object>>();
            }
        }

        return null;
    }

    private static Type? GetAwaitedResultType(Type type)
    {
        const BindingFlags members = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static;
        var awaiter = type.GetMethod("GetAwaiter", members, Type.EmptyTypes)?.ReturnType;
        if (awaiter is null || !typeof(INotifyCompletion).IsAssignableFrom(awaiter) ||
            awaiter.GetProperty("IsCompleted", members, binder: null, typeof(bool), Type.EmptyTypes, modifiers: null) is null)
        {
            return null;
        }

        return awaiter.GetMethod("GetResult", members, Type.EmptyTypes)?.ReturnType;
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

    private static object WrapChannel<T>(object channel, ClaimsPrincipal? user) =>
        new ScopedChannelReader<T>((ChannelReader<T>)channel, user);

    private sealed class ScopedChannelReader<T>(ChannelReader<T> reader, ClaimsPrincipal? user) : ChannelReader<T>
    {
        public override Task Completion
        {
            get
            {
                using var scope = new OrleansRequestContextScope(user);
                return reader.Completion;
            }
        }

        public override bool CanCount
        {
            get
            {
                using var scope = new OrleansRequestContextScope(user);
                return reader.CanCount;
            }
        }

        public override bool CanPeek
        {
            get
            {
                using var scope = new OrleansRequestContextScope(user);
                return reader.CanPeek;
            }
        }

        public override int Count
        {
            get
            {
                using var scope = new OrleansRequestContextScope(user);
                return reader.Count;
            }
        }

        public override bool TryRead([MaybeNullWhen(false)] out T item)
        {
            using var scope = new OrleansRequestContextScope(user);
            return reader.TryRead(out item);
        }

        public override bool TryPeek([MaybeNullWhen(false)] out T item)
        {
            using var scope = new OrleansRequestContextScope(user);
            return reader.TryPeek(out item);
        }

        public override async ValueTask<bool> WaitToReadAsync(CancellationToken cancellationToken = default)
        {
            using var scope = new OrleansRequestContextScope(user);
            return await reader.WaitToReadAsync(cancellationToken);
        }

        public override async ValueTask<T> ReadAsync(CancellationToken cancellationToken = default)
        {
            using var scope = new OrleansRequestContextScope(user);
            return await reader.ReadAsync(cancellationToken);
        }

        public override IAsyncEnumerable<T> ReadAllAsync(CancellationToken cancellationToken = default)
        {
            using var scope = new OrleansRequestContextScope(user);
            return new ScopedStream<T>(reader.ReadAllAsync(cancellationToken), user);
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
