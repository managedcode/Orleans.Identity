using System.Threading.Tasks;
using ManagedCode.Communication;
using ManagedCode.Orleans.Identity.Core.Models;
using Orleans;

namespace ManagedCode.Orleans.Identity.Core.Interfaces.TokenGrains;

public interface IEmailVerificationTokenGrain : IBaseTokenGrain
{
}