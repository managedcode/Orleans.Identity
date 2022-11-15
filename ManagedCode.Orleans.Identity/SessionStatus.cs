using System;
using System.Text.Json.Serialization;

namespace ManagedCode.Orleans.Identity;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum SessionStatus
{
    Active,
    Paused,
    Closed
}

public static class IdentityConstants
{
    public static string AUTH_TOKEN = "MC-AUTH-TOKEN";
    public static string AUTHENTICATION_TYPE = "MC-AUTH";
}