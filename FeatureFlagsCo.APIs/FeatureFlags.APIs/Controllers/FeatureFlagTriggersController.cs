using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FeatureFlags.APIs.Models;
using FeatureFlags.APIs.Services;
using FeatureFlags.APIs.ViewModels.FeatureFlagTrigger;
using FeatureFlagsCo.MQ;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;

namespace FeatureFlags.APIs.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class FeatureFlagTriggersController : ControllerBase
    {
        private readonly ILogger<FeatureFlagTriggersController> _logger;
        private readonly INoSqlService _noSqlDbService;
        private readonly IDistributedCache _redisCache;
        private readonly IAuditLogMqService _auditLogService;


        public FeatureFlagTriggersController(ILogger<FeatureFlagTriggersController> logger,
            INoSqlService noSqlDbService,
            IDistributedCache redisCache, IAuditLogMqService auditLogService)
        {
            _logger = logger;
            _noSqlDbService = noSqlDbService;
            _redisCache = redisCache;
            _auditLogService = auditLogService;
        }

        [HttpGet]
        [Route("{featureFlagId}")]
        public async Task<List<FeatureFlagTriggerViewModel>> GetByFeatureFlagId(string featureFlagId)
        {
            var triggers = await _noSqlDbService.GetFlagTriggersByFfIdAsync(featureFlagId);
            return triggers.Select(fft => new FeatureFlagTriggerViewModel()
            {
                Id = fft._Id,
                Type = (FeatureFlagTriggerTypeEnum)fft.Type,
                Action = (FeatureFlagTriggerActionEnum)fft.Action,
                Times = fft.Times,
                Status = (FeatureFlagTriggerStatusEnum)fft.Status,
                FeatureFlagId = fft.FeatureFlagId,
                Description = fft.Description,
                Token = this.ObscureToken(fft.Token),
                LastTriggeredAt = fft.LastTriggeredAt,
                UpdatedAt = fft.UpdatedAt
            }).ToList();
        }

        [HttpPut]
        [Route("{id}/{featureFlagId}/{status}")]
        public async Task<FeatureFlagTriggerViewModel> UpdateFeatureFlagTriggerStatus(string id, string featureFlagId, FeatureFlagTriggerStatusEnum status)
        {
            FeatureFlagTrigger fft;
            switch (status) 
            {
                case FeatureFlagTriggerStatusEnum.Enabled:
                    fft = await _noSqlDbService.EnableFlagTriggerAsync(id, featureFlagId);
                    break;
                case FeatureFlagTriggerStatusEnum.Disabled:
                    fft = await _noSqlDbService.DisableFlagTriggerAsync(id, featureFlagId);
                    break;
                case FeatureFlagTriggerStatusEnum.Archived:
                    fft = await _noSqlDbService.ArchiveFlagTriggerAsync(id, featureFlagId);
                    break;
                default:
                    throw new NotSupportedException("action is not supported");
            }

            return new FeatureFlagTriggerViewModel()
            {
                Id = fft._Id,
                Type = (FeatureFlagTriggerTypeEnum)fft.Type,
                Action = (FeatureFlagTriggerActionEnum)fft.Action,
                Times = fft.Times,
                Status = (FeatureFlagTriggerStatusEnum)fft.Status,
                FeatureFlagId = fft.FeatureFlagId,
                Description = fft.Description,
                Token = this.ObscureToken(fft.Token),
                UpdatedAt = fft.UpdatedAt
            };
        }

        [HttpPut]
        [Route("token/{id}/{featureFlagId}")]
        public async Task<FeatureFlagTriggerViewModel> UpdateFeatureFlagTriggerStatus(string id, string featureFlagId)
        {
            var fft = await _noSqlDbService.ResetFlagTriggerTokenAsync(id, featureFlagId);

            return new FeatureFlagTriggerViewModel()
            {
                Id = fft._Id,
                Type = (FeatureFlagTriggerTypeEnum)fft.Type,
                Action = (FeatureFlagTriggerActionEnum)fft.Action,
                Times = fft.Times,
                Status = (FeatureFlagTriggerStatusEnum)fft.Status,
                FeatureFlagId = fft.FeatureFlagId,
                Description = fft.Description,
                Token = fft.Token,
                UpdatedAt = fft.UpdatedAt,
                LastTriggeredAt = fft.LastTriggeredAt
            };
        }
        [AllowAnonymous]
        [HttpGet]
        [Route("trigger/{token}")]
        public async Task<dynamic> Trigger(string token)
        {
            try
            {
                await _noSqlDbService.TriggerFeatureFlagByFlagTriggerAsync(token);
                return StatusCode(StatusCodes.Status200OK);
            }
            catch (NotSupportedException) 
            {
                _logger.LogWarning($"There is an error in the trigger. Token: {token}");
                return StatusCode(StatusCodes.Status200OK);
            }
            catch (InvalidOperationException)
            {
                _logger.LogWarning($"Invalid Token: {token}");
                return StatusCode(StatusCodes.Status200OK);
            }
        }

        [HttpPost]
        [Route("")]
        public async Task<FeatureFlagTriggerViewModel> CreateFeatureFlagTrigger(
            [FromBody] FeatureFlagTriggerViewModel param)
        {
            return await _noSqlDbService.CreateFlagTriggerAsync(param);
        }

        private string ObscureToken(string token)
        {
            return token.Substring(0, 5) + "************************";
        }
    }
}