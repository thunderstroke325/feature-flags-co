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
using FeatureFlags.APIs.ViewModels.FeatureFlagCommit;

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
        private readonly MongoDbFeatureFlagCommitService _featureFlagCommitService;

        public PendingChangesController(
            ILogger<PendingChangesController> logger,
            IDistributedCache redisCache,
            IEnvironmentService envService,
            INoSqlService noSqlDbService,
            MongoDbFeatureFlagCommitService featureFlagCommitService)
        {
            _logger = logger;
            _redisCache = redisCache;
            _envService = envService;
            _noSqlDbService = noSqlDbService;
            _featureFlagCommitService = featureFlagCommitService;
        }

        [HttpPost]
        [Route("")]
        public async Task<IActionResult> CreatePendingChanges([FromBody] CreateApproveRequestParam param)
        {
            try
            {
                var currentUserId = this.HttpContext.User.Claims.FirstOrDefault(p => p.Type == "UserId").Value;
                if (await _envService.CheckIfUserHasRightToReadEnvAsync(currentUserId, param.FeatureFlagParam.EnvironmentId))
                {
                    await _noSqlDbService.CreateApproveRequestAsync(param, currentUserId);
                    return StatusCode(StatusCodes.Status200OK);
                }

                return StatusCode(StatusCodes.Status401Unauthorized, new Response { Code = "Error", Message = "Unauthorized" });
            }
            catch (Exception exp)
            {
                _logger.LogError(exp, JsonConvert.SerializeObject(param));

                return StatusCode(StatusCodes.Status500InternalServerError, new Response { Code = "Error", Message = "Internal Error" });
            }
        }

        [HttpPost]
        [Route("/ApplyRequest")]
        public async Task<IActionResult> ApplyRequestAsync(ApplyRequestParam arParam)
        {
            try
            {
                var currentUserId = this.HttpContext.User.Claims.FirstOrDefault(p => p.Type == "UserId").Value;
                if (await _envService.CheckIfUserHasRightToReadEnvAsync(currentUserId, arParam.EnvironmentId))
                {
                    var arResult = await _noSqlDbService.ApplyApprovalRequestAsync(arParam, currentUserId);
                    if(arResult != null && arResult.Id == arParam.FeatureFlagId)
                    {
                        await _redisCache.SetStringAsync(arResult.Id, JsonConvert.SerializeObject(arResult));
                        return StatusCode(StatusCodes.Status200OK);
                    }
                }
                return StatusCode(StatusCodes.Status401Unauthorized, new Response { Code = "Error", Message = "Unauthorized" });
            }
            catch (Exception exp)
            {
                _logger.LogError(exp, JsonConvert.SerializeObject(arParam));

                return StatusCode(StatusCodes.Status500InternalServerError, new Response { Code = "Error", Message = "Internal Error" });
            }
        }
    }
}
