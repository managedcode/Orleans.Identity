using System;
using Orleans;
using Orleans.Runtime;

namespace ManagedCode.Orleans.Identity.Models;

[GenerateSerializer]
public class TokenModel
{
    [Id(0)] 
    public string Value { get; set; }
    
    [Id(1)]
    public GrainId UserGrainId { get; set; }
    
    [Id(2)]
    public DateTimeOffset ExpirationDate { get; set; }
}