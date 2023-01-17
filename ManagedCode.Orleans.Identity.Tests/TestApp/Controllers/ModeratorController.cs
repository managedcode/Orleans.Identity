using ManagedCode.Orleans.Identity.Extensions;
using ManagedCode.Orleans.Identity.Tests.Cluster.Grains.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ManagedCode.Orleans.Identity.Tests.TestApp.Controllers
{
    [Route("moderatorController")]
    public class ModeratorController : ControllerBase
    {
        private readonly IClusterClient _clusterClient;

        public ModeratorController(IClusterClient clusterClient)
        {
            _clusterClient = clusterClient;
        }

        [HttpGet]
        public async Task<ActionResult<string>> GetInfo()
        {
            var userId = User.GetGrainId();
            var moderatorGrain = _clusterClient.GetGrain<IModeratorGrain>(userId);
            return await moderatorGrain.GetInfo();
        }

        [HttpGet("getModerators")]
        public async Task<ActionResult<string>> GetModerators()
        {
            var userId = User.GetGrainId();
            var moderatorGrain = _clusterClient.GetGrain<IModeratorGrain>(userId);
            return await moderatorGrain.GetModerators();
        }
    }
}
