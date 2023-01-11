using ManagedCode.Orleans.Identity.Shared.Enums;
using Orleans;
using System;
using System.Collections.Generic;

namespace ManagedCode.Orleans.Identity.Entities;

[GenerateSerializer]
public class SessionEntity
{
    [Id(0)]
    public string Id { get; set; }

    [Id(1)]
    public string Email { get; set; }

    [Id(2)]
    public DateTime CreatedDate { get; set; }

    [Id(3)]
    public DateTime LastAccess { get; set; }

    [Id(4)]
    public DateTime? ClosedDate { get; set; }

    [Id(5)]
    public SessionStatus Status { get; set; }

    [Id(6)]
    public List<string> Roles { get; set; }
}
