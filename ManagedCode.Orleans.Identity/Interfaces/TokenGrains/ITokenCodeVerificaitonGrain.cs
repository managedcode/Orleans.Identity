using ManagedCode.Communication;
using System.Threading.Tasks;

namespace ManagedCode.Orleans.Identity.Interfaces.TokenGrains
{
    public interface ITokenCodeVerificaitonGrain : IBaseTokenGrain
    {
        ValueTask<Result> VerifyAsync();
    }
}
