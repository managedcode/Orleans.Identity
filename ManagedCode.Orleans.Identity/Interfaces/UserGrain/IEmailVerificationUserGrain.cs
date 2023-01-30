using Orleans;

namespace ManagedCode.Orleans.Identity.Interfaces.UserGrain
{
    public interface IEmailVerificationUserGrain : IGrainWithStringKey
    {
        ValueTask<>
    }
}
