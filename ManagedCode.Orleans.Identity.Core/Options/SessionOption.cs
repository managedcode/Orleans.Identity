using System;

namespace ManagedCode.Orleans.Identity.Core.Options;

public class SessionOption
{
    public bool ClearStateOnClose { get; set; } = true;

    public TimeSpan SessionLifetime { get; set; } = TimeSpan.FromDays(20);
}