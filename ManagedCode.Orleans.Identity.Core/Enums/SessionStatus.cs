﻿using System.Text.Json.Serialization;

namespace ManagedCode.Orleans.Identity.Core.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum SessionStatus
{
    Active,
    Paused,
    Closed
}