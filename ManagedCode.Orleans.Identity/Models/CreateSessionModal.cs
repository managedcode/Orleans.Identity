using Orleans;
using System.Collections.Generic;

namespace ManagedCode.Orleans.Identity.Models;

[GenerateSerializer]
public class CreateSessionModal 
{
    [Id(0)]
    public string Email { get; set; }

    [Id(1)]
    public List<string> Roles { get; set; }
}