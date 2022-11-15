using Orleans;

namespace ManagedCode.Orleans.Identity;

[GenerateSerializer]
public class SessionInfo 
{
    public string Phone { get; set; }
    public string AppVersion { get; set; }
    public string DeviceId { get; set; }
    public string LanguageISO { get; set; }
    public string CountryISO { get; set; }
    public string DeviceName { get; set; }
    public string DevicePushToken { get; set; }
    public DevicePlatform DevicePlatform { get; set; }
    
}