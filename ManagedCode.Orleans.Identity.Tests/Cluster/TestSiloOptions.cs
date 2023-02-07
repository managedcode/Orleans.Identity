using ManagedCode.Orleans.Identity.Options;

namespace ManagedCode.Orleans.Identity.Tests.Cluster;

public static class TestSiloOptions
{
    public static SessionOption SessionOption { get; } = new();
}

public static class ShortLifetimeSiloOptions
{
    public static SessionOption SessionOption { get; } = new() { SessionLifetime = TimeSpan.FromMinutes(1).Add(TimeSpan.FromSeconds(40))};
}