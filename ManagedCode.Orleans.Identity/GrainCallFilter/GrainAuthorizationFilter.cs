using Orleans;
using System.Threading.Tasks;

namespace ManagedCode.Orleans.Identity.GrainCallFilter
{
    // Test filter
    // TODO: Get sessionID from request context and check if session valid
    public class GrainAuthorizationFilter : IIncomingGrainCallFilter
    {
        public Task Invoke(IIncomingGrainCallContext context)
        {
            return Task.FromResult(0);
        }
    }
}
