using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ManagedCode.Identity.Core;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace ManagedCode.Identity.TestApp.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {


        private readonly ILogger<WeatherForecastController> _logger;
        private readonly UserManager<InMemoryIdentityUser> _userManager;

        public WeatherForecastController(ILogger<WeatherForecastController> logger, UserManager<InMemoryIdentityUser> userManager)
        {
            _logger = logger;
            _userManager = userManager;
        }

        [HttpGet]
        public async Task<IEnumerable<string>> Get()
        {
            var user = await _userManager.CreateAsync(new InMemoryIdentityUser());
            return Enumerable.Range(1, 5)
                .Select(index => index.ToString())
                .ToArray();
        }
    }
}