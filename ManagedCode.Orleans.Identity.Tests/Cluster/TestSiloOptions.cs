using ManagedCode.Orleans.Identity.Server.Options;

namespace ManagedCode.Orleans.Identity.Tests.Cluster;

public static class TestSiloOptions
{
    public static SessionOption SessionOption { get; } = new();
}