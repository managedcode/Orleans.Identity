using System;
using ManagedCode.Orleans.Identity.Shared.Enums;
using Orleans;

namespace ManagedCode.Orleans.Identity.Models;

[GenerateSerializer]
public class SessionModel
{
    [Id(0)]
    public string Id { get; set; }

    [Id(1)]
    public DateTime CreatedDate { get; set; }

    [Id(2)]
    public DateTime LastAccess { get; set; }

    [Id(3)]
    public DateTime? ClosedDate { get; set; }

    [Id(4)]
    public SessionStatus Status { get; set; }

    [Id(5)]
    public bool IsActive { get; set; }

}

