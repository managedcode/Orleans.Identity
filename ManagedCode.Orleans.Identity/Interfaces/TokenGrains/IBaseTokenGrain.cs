using System.Threading.Tasks;
using ManagedCode.Communication;
using ManagedCode.Orleans.Identity.Models;
using Orleans;

namespace ManagedCode.Orleans.Identity.Interfaces.TokenGrains;

public interface IBaseTokenGrain : IGrainWithStringKey
{
    ValueTask<Result> CreateAsync(CreateTokenModel createModel);
}