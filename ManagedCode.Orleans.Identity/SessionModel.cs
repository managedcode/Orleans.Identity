using System;
using ManagedCode.Communication;
using Orleans;

namespace ManagedCode.Orleans.Identity;

[GenerateSerializer]
public class SessionModel
{
    [Id(0)]
    public string Id { get; set; }
    [Id(1)]
    public string AccountId { get; set; }
    [Id(2)]
    public string DeviceId { get; set; }
    [Id(3)]
    public string DevicePushToken { get; set; }
    [Id(4)]
    public string AppVersion { get; set; }
    [Id(5)]
    public string DeviceName { get; set; }
    [Id(6)]
    public DateTime CreatedDate { get; set; }
    [Id(7)]
    public DateTime LastAccess { get; set; }
    [Id(8)]
    public DateTime? ClosedDate { get; set; }
    [Id(9)]
    public SessionStatus Status { get; set; }
    [Id(10)]
    public DevicePlatform DevicePlatform { get; set; } 

}

