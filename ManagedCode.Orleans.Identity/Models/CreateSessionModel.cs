using Orleans;
using Orleans.Runtime;
using System.Collections.Generic;

namespace ManagedCode.Orleans.Identity.Models;

[GenerateSerializer]
public class CreateSessionModel 
{

    [Id(0)]
    public GrainId UserGrainId { get; set; }

    [Id(1)]
    public Dictionary<string, string> UserData { get; set; }
}