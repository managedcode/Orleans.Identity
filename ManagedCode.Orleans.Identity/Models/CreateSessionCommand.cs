using ManagedCode.Orleans.Identity.Models.Enums;
using Orleans;

namespace ManagedCode.Orleans.Identity.Models;

[GenerateSerializer]
public class SessionInfo 
{
    [Id(0)]
    public string Phone { get; set; }
    [Id(1)]
    public string AppVersion { get; set; }
    [Id(2)]
    public string DeviceId { get; set; }
    [Id(3)]
    public string LanguageISO { get; set; }
    [Id(4)]
    public string CountryISO { get; set; }
    [Id(5)]
    public string DeviceName { get; set; }
    [Id(6)]
    public string DevicePushToken { get; set; }
    [Id(7)]
    public DevicePlatform DevicePlatform { get; set; }
    
}