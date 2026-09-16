using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Orleans.Metadata;
using Orleans.Runtime;
using Orleans.Serialization.Invocation;

namespace ManagedCode.Orleans.Identity.Server.GrainCallFilter;

/// <summary>Exposes the original streamed grain operation to authorization policies.</summary>
internal sealed class StreamingGrainCallContext : IIncomingGrainCallContext
{
    private readonly IIncomingGrainCallContext transport;

    public StreamingGrainCallContext(IIncomingGrainCallContext transport, IInvokable request)
    {
        this.transport = transport;
        Request = request;
        Grain = transport.TargetContext.GrainInstance
            ?? throw new UnauthorizedAccessException("Streaming target is unavailable.");
        InterfaceMethod = request.GetMethod();
        var declaringType = InterfaceMethod.DeclaringType
            ?? throw new UnauthorizedAccessException("Streaming operation is unavailable.");
        if (!declaringType.IsInterface || !declaringType.IsInstanceOfType(Grain) ||
            !InterfaceMethod.ReturnType.IsGenericType ||
            InterfaceMethod.ReturnType.GetGenericTypeDefinition() != typeof(IAsyncEnumerable<>))
        {
            throw new UnauthorizedAccessException("Streaming operation is invalid.");
        }

        request.SetTarget(transport.TargetContext);
        var map = Grain.GetType().GetInterfaceMap(declaringType);
        var definition = InterfaceMethod.IsGenericMethod ? InterfaceMethod.GetGenericMethodDefinition() : InterfaceMethod;
        var index = Array.FindIndex(map.InterfaceMethods, method => method == definition);
        if (index < 0)
        {
            throw new UnauthorizedAccessException("Streaming implementation is unavailable.");
        }
        ImplementationMethod = map.TargetMethods[index];
        if (ImplementationMethod.IsGenericMethodDefinition)
        {
            ImplementationMethod = ImplementationMethod.MakeGenericMethod(InterfaceMethod.GetGenericArguments());
        }
        InterfaceType = transport.TargetContext.ActivationServices
            .GetRequiredService<GrainInterfaceTypeResolver>().GetGrainInterfaceType(request.GetInterfaceType());
    }

    public IInvokable Request { get; }
    public object Grain { get; }
    public GrainId? SourceId => transport.SourceId;
    public GrainId TargetId => transport.TargetId;
    public GrainInterfaceType InterfaceType { get; }
    public string InterfaceName => Request.GetInterfaceName();
    public string MethodName => Request.GetMethodName();
    public MethodInfo InterfaceMethod { get; }
    public MethodInfo ImplementationMethod { get; }
    public IGrainContext TargetContext => transport.TargetContext;
    public object? Result { get => transport.Result; set => transport.Result = value; }
    public Response? Response { get => transport.Response; set => transport.Response = value; }
    public Task Invoke() => throw new InvalidOperationException("Authorization must not invoke the streaming transport.");
}
