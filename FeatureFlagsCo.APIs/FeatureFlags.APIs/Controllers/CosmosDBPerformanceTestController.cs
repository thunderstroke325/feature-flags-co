using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FeatureFlags.APIs.Authentication;
using FeatureFlags.APIs.Repositories;
using FeatureFlags.APIs.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;

namespace FeatureFlags.APIs.Controllers
{
    //[Authorize(Roles = UserRoles.Admin)]
    [ApiController]
    [Route("[controller]")]
    public class CosmosDBPerformanceTestController : ControllerBase
    {
        private static readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        private readonly INoSqlService _cosmosdbService;
        //private readonly ILaunchDarklyService _ldService;

        public CosmosDBPerformanceTestController(
            INoSqlService cosmosdbService)
            //ILaunchDarklyService ldService)
        {
            _cosmosdbService = cosmosdbService;
        }

        [HttpGet]
        public async Task<string> Get()
        {
            var ff = await _cosmosdbService.GetFeatureFlagAsync("FF__MQ==__cosmosdb-feature");
            return ff.Id;
        }

    }
}
