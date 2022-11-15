using System;
using ManagedCode.Communication;
using Orleans;

namespace ManagedCode.Orleans.Identity;

[GenerateSerializer]
public class SessionModel
{
    public string Id { get; set; }
    public string AccountId { get; set; }
    public string DeviceId { get; set; }
    public string DevicePushToken { get; set; }
    public string AppVersion { get; set; }
    public string DeviceName { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime LastAccess { get; set; }
    public DateTime? ClosedDate { get; set; }
    public SessionStatus Status { get; set; }
    public DevicePlatform DevicePlatform { get; set; } 

}

