using System.Collections.Generic;
using Orleans;
using Orleans.Runtime;

namespace ManagedCode.Orleans.Identity.Models;

[GenerateSerializer]
public class CreateSessionModel
{
    [Id(0)]
    public GrainId UserGrainId { get; set; }

    // TODO: Do smth with dictionary user cant have more than one role
    [Id(1)]
    public Dictionary<string, HashSet<string>> UserData { get; set; } = new();
}