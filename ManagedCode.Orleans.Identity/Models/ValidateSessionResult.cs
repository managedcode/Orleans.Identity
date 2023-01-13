using System.Collections.Generic;
using Orleans;

namespace ManagedCode.Orleans.Identity.Models;

public class ValidateSessionResult
{
    [Id(0)]
    public string Email { get; set; }

    [Id(1)]
    public List<string> Roles { get; set; }
}