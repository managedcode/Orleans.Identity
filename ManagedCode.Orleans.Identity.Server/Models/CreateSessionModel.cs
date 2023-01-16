using System.Collections.Generic;
using Orleans;
using Orleans.Runtime;

namespace ManagedCode.Orleans.Identity.Server.Models;

[GenerateSerializer]
public class CreateSessionModel
{
    [Id(0)]
    public GrainId UserGrainId { get; set; }
    
    [Id(1)]
    public Dictionary<string, HashSet<string>> UserData { get; set; } = new();

    public CreateSessionModel()
    {
        
    }
    
    public CreateSessionModel(GrainId grainId)
    {
        UserGrainId = grainId;
    }
    
    public CreateSessionModel(Dictionary<string, HashSet<string>> userData)
    {
        UserData = userData;
    }
    
    public CreateSessionModel(GrainId grainId, Dictionary<string, HashSet<string>> userData)
    {
        UserGrainId = grainId;
        UserData = userData;
    }
    

    public CreateSessionModel AddUserGrainId(GrainId grainId)
    {
        UserGrainId = grainId;
        return this;
    }
    
    public CreateSessionModel AddProperty(string key, string value)
    {
        if (UserData.TryGetValue(key, out HashSet<string>? hashSet))
        {
            hashSet.Add(value);
        }
        else
        {
            UserData[key] = new HashSet<string> { value };
        }

        return this;
    }
    
    public CreateSessionModel AddProperty(string key, IEnumerable<string> values)
    {
        if (UserData.TryGetValue(key, out HashSet<string>? hashSet))
        {
            foreach (var value in values)
            {
                hashSet.Add(value);
            }
        }
        else
        {
            UserData[key] = new HashSet<string>(values);
        }

        return this;
    }
}