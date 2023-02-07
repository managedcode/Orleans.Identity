using System.Threading.Tasks;
using ManagedCode.Communication;
using ManagedCode.Orleans.Identity.Models;
using Orleans;

namespace ManagedCode.Orleans.Identity.Interfaces.TokenGrains;

public interface IEmailVerificationTokenGrain : IBaseTokenGrain
{
}