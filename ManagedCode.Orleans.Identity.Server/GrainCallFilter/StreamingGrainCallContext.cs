using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Orleans.Metadata;
using Orleans.Runtime;
using Orleans.Serialization.Invocation;

namespace ManagedCode.Orleans.Identity.Server.GrainCallFilter;

/// <summary>Exposes the original streamed grain operation to authorization policies.</summary>
internal sealed class StreamingGrainCallContext(
    IIncomingGrainCallContext transport, StreamingGrainCallContext.Operation operation) : IIncomingGrainCallContext
{
    public static Operation ResolveOperation(IGrainContext target, IInvokable request)
    {
        var grain = target.GrainInstance
            ?? throw new UnauthorizedAccessException("Streaming target is unavailable.");
        var interfaceMethod = request.GetMethod();
        var declaringType = interfaceMethod.DeclaringType
            ?? throw new UnauthorizedAccessException("Streaming operation is unavailable.");
        if (!declaringType.IsInterface || !declaringType.IsInstanceOfType(grain) ||
            !interfaceMethod.ReturnType.IsGenericType ||
            interfaceMethod.ReturnType.GetGenericTypeDefinition() != typeof(IAsyncEnumerable<>))
        {
            throw new UnauthorizedAccessException("Streaming operation is invalid.");
        }

        request.SetTarget(target);
        var map = grain.GetType().GetInterfaceMap(declaringType);
        var definition = interfaceMethod.IsGenericMethod ? interfaceMethod.GetGenericMethodDefinition() : interfaceMethod;
        var index = Array.IndexOf(map.InterfaceMethods, definition);
        if (index < 0)
        {
            throw new UnauthorizedAccessException("Streaming implementation is unavailable.");
        }
        var implementationMethod = map.TargetMethods[index];
        if (implementationMethod.IsGenericMethodDefinition)
        {
            implementationMethod = implementationMethod.MakeGenericMethod(interfaceMethod.GetGenericArguments());
        }
        var interfaceType = target.ActivationServices
            .GetRequiredService<GrainInterfaceTypeResolver>().GetGrainInterfaceType(request.GetInterfaceType());

        return new Operation(request, interfaceMethod, implementationMethod, interfaceType);
    }

    public IInvokable Request => operation.Request;
    public object Grain => TargetContext.GrainInstance!;
    public GrainId? SourceId => transport.SourceId;
    public GrainId TargetId => transport.TargetId;
    public GrainInterfaceType InterfaceType => operation.InterfaceType;
    public string InterfaceName => Request.GetInterfaceName();
    public string MethodName => Request.GetMethodName();
    public MethodInfo InterfaceMethod => operation.InterfaceMethod;
    public MethodInfo ImplementationMethod => operation.ImplementationMethod;
    public IGrainContext TargetContext => transport.TargetContext;
    public object? Result { get => transport.Result; set => transport.Result = value; }
    public Response? Response { get => transport.Response; set => transport.Response = value; }
    public Task Invoke() => throw new InvalidOperationException("Authorization must not invoke the streaming transport.");

    internal readonly record struct Operation(
        IInvokable Request, MethodInfo InterfaceMethod, MethodInfo ImplementationMethod, GrainInterfaceType InterfaceType);
}
