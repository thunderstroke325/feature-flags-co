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

        
        [HttpGet]
        [Route("GetApprovalRequests")]
        public async Task<IActionResult> GetApprovalRequests(string featureFlagId)
        {
            try
            {
                var currentUserId = this.HttpContext.User.Claims.FirstOrDefault(p => p.Type == "UserId").Value;
                var ffcs = await _noSqlDbService.GetApprovalRequestsAsync(featureFlagId);
                if (ffcs != null && ffcs.Count > 0)
                {
                    if (!(await _envService.CheckIfUserHasRightToReadEnvAsync(currentUserId, ffcs[0].EnvironmentId)))
                    {
                        return StatusCode(StatusCodes.Status401Unauthorized, new Response { Code = "Error", Message = "Unauthorized" });
                    }
                }
                return StatusCode(StatusCodes.Status200OK, ffcs);
            }
            catch (Exception exp)
            {
                _logger.LogError(exp, JsonConvert.SerializeObject(new { featureFlagId = featureFlagId }));
                return StatusCode(StatusCodes.Status500InternalServerError, new Response { Code = "Error", Message = "Internal Error" });
            }
        }

        [HttpGet]
        [Route("GetApprovalRequests")]
        public async Task<IActionResult> GetApprovalRequest(string featureFlagId)
        {
            try
            {
                var currentUserId = this.HttpContext.User.Claims.FirstOrDefault(p => p.Type == "UserId").Value;
                var ffc = await _noSqlDbService.GetApprovalRequestAsync(featureFlagId);
                if (ffc != null)
                {
                    if (!(await _envService.CheckIfUserHasRightToReadEnvAsync(currentUserId, ffc.EnvironmentId)))
                    {
                        return StatusCode(StatusCodes.Status401Unauthorized, new Response { Code = "Error", Message = "Unauthorized" });
                    }
                }
                return StatusCode(StatusCodes.Status200OK, ffc);
            }
            catch (Exception exp)
            {
                _logger.LogError(exp, JsonConvert.SerializeObject(new { featureFlagId = featureFlagId }));
                return StatusCode(StatusCodes.Status500InternalServerError, new Response { Code = "Error", Message = "Internal Error" });
            }
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
        [Route("ApproveRequest")]
        public async Task<IActionResult> ApproveRequest(ApproveRequestParam param)
        {
            try
            {
                var currentUserId = this.HttpContext.User.Claims.FirstOrDefault(p => p.Type == "UserId").Value;
                if (await _envService.CheckIfUserHasRightToReadEnvAsync(currentUserId, param.EnvironmentId))
                {
                    var result = await _noSqlDbService.ApproveApprovalRequestAsync(param, currentUserId);
                    if (result == true)
                    {
                        return StatusCode(StatusCodes.Status200OK);
                    }
                    else
                    {
                        _logger.LogError("Unkown approve request error", JsonConvert.SerializeObject(param));
                        return StatusCode(StatusCodes.Status500InternalServerError, "Unknown approve request error");
                    }
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
        [Route("ChangeApprovalRequest")]
        public async Task<IActionResult> ChangeApprovalRequest(ChangeRequestParam param)
        {
            try
            {
                var currentUserId = this.HttpContext.User.Claims.FirstOrDefault(p => p.Type == "UserId").Value;
                if (await _envService.CheckIfUserHasRightToReadEnvAsync(currentUserId, param.EnvironmentId))
                {
                    var result = await _noSqlDbService.ChangeApprovalRequestAsync(param, currentUserId);
                    if (result == true)
                    {
                        return StatusCode(StatusCodes.Status200OK);
                    }
                    else
                    {
                        _logger.LogError("Unkown approve request error", JsonConvert.SerializeObject(param));
                        return StatusCode(StatusCodes.Status500InternalServerError, "Unknown approve request error");
                    }
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
        [Route("DeclineApprovalRequest")]
        public async Task<IActionResult> DeclineApprovalRequest(DeclineRequestParam param)
        {
            try
            {
                var currentUserId = this.HttpContext.User.Claims.FirstOrDefault(p => p.Type == "UserId").Value;
                if (await _envService.CheckIfUserHasRightToReadEnvAsync(currentUserId, param.EnvironmentId))
                {
                    var result = await _noSqlDbService.DeclineApprovalRequestAsync(param, currentUserId);
                    if (result == true)
                    {
                        return StatusCode(StatusCodes.Status200OK);
                    }
                    else
                    {
                        _logger.LogError("Unkown approve request error", JsonConvert.SerializeObject(param));
                        return StatusCode(StatusCodes.Status500InternalServerError, "Unknown approve request error");
                    }
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
        [Route("ApplyRequest")]
        public async Task<IActionResult> ApplyRequest(ApplyRequestParam param)
        {
            try
            {
                var currentUserId = this.HttpContext.User.Claims.FirstOrDefault(p => p.Type == "UserId").Value;
                if (await _envService.CheckIfUserHasRightToReadEnvAsync(currentUserId, param.EnvironmentId))
                {
                    var arResult = await _noSqlDbService.ApplyApprovalRequestAsync(param, currentUserId);
                    if(arResult != null && arResult.Id == param.FeatureFlagId)
                    {
                        await _redisCache.SetStringAsync(arResult.Id, JsonConvert.SerializeObject(arResult));
                        return StatusCode(StatusCodes.Status200OK);
                    }
                }
                return StatusCode(StatusCodes.Status401Unauthorized, new Response { Code = "Error", Message = "Unauthorized" });
            }
            catch (Exception exp)
            {
                _logger.LogError(exp, JsonConvert.SerializeObject(param));

                return StatusCode(StatusCodes.Status500InternalServerError, new Response { Code = "Error", Message = "Internal Error" });
            }
        }
    }
}
