using FeatureFlags.APIs.Models;
using FeatureFlags.APIs.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;

using System.Linq;
using System.Threading.Tasks;

using FeatureFlags.APIs.Authentication;

namespace FeatureFlags.APIs.Controllers
{
    //[Authorize(Roles = UserRoles.Admin)]
    [Authorize]
    [ApiController]
    [Route("[controller]")]
    public class PendingChangesController : ControllerBase
    {
        private readonly ILogger<PendingChangesController> _logger;
        private readonly IDistributedCache _redisCache;
        private readonly IEnvironmentService _envService;
        private readonly INoSqlService _noSqlDbService;
        private readonly FFPendingChangesService _pendingChangesService;

        public PendingChangesController(
            ILogger<PendingChangesController> logger,
            IDistributedCache redisCache,
            IEnvironmentService envService,
            INoSqlService noSqlDbService,
            FFPendingChangesService pendingChangesService)
        {
            _logger = logger;
            _redisCache = redisCache;
            _envService = envService;
            _noSqlDbService = noSqlDbService;
            _pendingChangesService = pendingChangesService;
        }

        [HttpPost]
        [Route("")]
        public async Task<dynamic> CreatePendingChanges([FromBody] FFPendingChanges param)
        {
            try
            {
                var currentUserId = this.HttpContext.User.Claims.FirstOrDefault(p => p.Type == "UserId").Value;
                if (await _envService.CheckIfUserHasRightToReadEnvAsync(currentUserId, param.EnvId) || await _noSqlDbService.GetFlagAsync(param.FeatureFlagId) == null)
                {
                    var pendingChanges = await _pendingChangesService.CreateAsync(param);
                    await _redisCache.SetStringAsync(pendingChanges.Id, JsonConvert.SerializeObject(pendingChanges));
                    return pendingChanges;
                }

                return StatusCode(StatusCodes.Status401Unauthorized, new Response { Status = "Error", Message = "Unauthorized" });
            }
            catch (Exception exp)
            {
                _logger.LogError(exp, JsonConvert.SerializeObject(param));

                return StatusCode(StatusCodes.Status500InternalServerError, new Response { Status = "Error", Message = "Internal Error" });
            }
        }
    }
}
