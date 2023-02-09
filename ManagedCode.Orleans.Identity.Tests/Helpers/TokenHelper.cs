using ManagedCode.Orleans.Identity.Models;
using Orleans.Runtime;

namespace ManagedCode.Orleans.Identity.Tests.Helpers;

public static class TokenHelper
{
    public static CreateTokenModel GenerateCreateTestTokenModel()
    {
        var randomValue = Guid.NewGuid().ToString();
        var randomUserGrainId = Guid.NewGuid().ToString();
        return new CreateTokenModel
        {
            Value = randomValue,
            UserGrainId = GrainId.Create("userGrain", randomUserGrainId),
            Lifetime = TimeSpan.FromMinutes(2)
        };
    }
    
    public static CreateTokenModel GenerateCreateTestTokenModel(string tokenValue)
    {
        var randomUserGrainId = Guid.NewGuid().ToString();
        return new CreateTokenModel
        {
            Value = tokenValue,
            UserGrainId = GrainId.Create("userGrain", randomUserGrainId),
            Lifetime = TimeSpan.FromMinutes(4)
        };
    }
    
    public static CreateTokenModel GenerateCreateTestTokenModel(string tokenValue, TimeSpan lifetime)
    {
        var randomUserGrainId = Guid.NewGuid().ToString();
        return new CreateTokenModel
        {
            Value = tokenValue,
            UserGrainId = GrainId.Create("userGrain", randomUserGrainId),
            Lifetime = lifetime
        };
    }
    
    public static CreateTokenModel GenerateCreateTestTokenModel(TimeSpan lifetime)
    {
        var randomValue = Guid.NewGuid().ToString();
        var randomUserGrainId = Guid.NewGuid().ToString();
        return new CreateTokenModel
        {
            Value = randomValue,
            UserGrainId = GrainId.Create("userGrain", randomUserGrainId),
            Lifetime = lifetime
        };
    }
}