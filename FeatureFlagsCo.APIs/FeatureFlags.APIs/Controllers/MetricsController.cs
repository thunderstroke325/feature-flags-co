using FeatureFlags.APIs.Authentication;
using FeatureFlags.APIs.Models;
using FeatureFlags.APIs.Services;
using FeatureFlags.APIs.ViewModels.Metrics;
using FeatureFlagsCo.MQ;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FeatureFlags.APIs.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class MetricsController : ControllerBase
    {
        private readonly ILogger<MetricsController> _logger;
        private readonly IEnvironmentService _envService;
        private readonly INoSqlService _noSqlDbService;
        private readonly MetricService _metricService;

        public MetricsController(
            ILogger<MetricsController> logger,
            IEnvironmentService envService,
            MetricService metricService,
            INoSqlService noSqlDbService)
        {
            _logger = logger;
            _envService = envService;
            _noSqlDbService = noSqlDbService;
            _metricService = metricService;
        }

        private List<string> ValidateMetric(MetricViewModel param) 
        {
            var validateErrors = new List<string>();
            if (string.IsNullOrWhiteSpace(param.EventName))
            {
                validateErrors.Add("事件名称");
            }

            if (param.EventType == EventType.Custom)
            {
                if (param.CustomEventTrackOption == CustomEventTrackOption.Undefined)
                {
                    validateErrors.Add("转化率或数值");
                }

                if (param.CustomEventSuccessCriteria == CustomEventSuccessCriteria.Undefined)
                {
                    validateErrors.Add("实验胜出标准");
                }
            }

            return validateErrors;
        }

        [HttpPost]
        [Route("")]
        public async Task<dynamic> CreateMetric([FromBody] MetricViewModel param)
        {
            try
            {
                var currentUserId = this.HttpContext.User.Claims.FirstOrDefault(p => p.Type == "UserId").Value;
                if (await _envService.CheckIfUserHasRightToReadEnvAsync(currentUserId, param.EnvId))
                {
                    var validateErrors = ValidateMetric(param);
                    if (validateErrors.Count > 0)
                    {
                        return StatusCode(StatusCodes.Status400BadRequest, new Response { Code = "Error", Messages = validateErrors });
                    }

                    var metric = new Metric 
                    {
                        Name = param.Name,
                        EnvId = param.EnvId,
                        Description = param.Description,
                        EventName = param.EventName,
                        EventType = param.EventType,
                        CustomEventTrackOption = param.CustomEventTrackOption,
                        MaintainerUserId = param.MaintainerUserId,
                        CustomEventUnit = param.CustomEventUnit,
                        CustomEventSuccessCriteria = param.CustomEventSuccessCriteria,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };

                    var newMetric = await _metricService.CreateAsync(metric);
                    param.Id = newMetric.Id;
                    return param;
                }

                return StatusCode(StatusCodes.Status401Unauthorized, new Response { Code = "Error", Message = "Unauthorized" });
            }
            catch (Exception exp)
            {
                _logger.LogError(exp, JsonConvert.SerializeObject(param));

                return StatusCode(StatusCodes.Status500InternalServerError, new Response { Code = "Error", Message = "Internal Error" });
            }
        }

        [HttpPut]
        [Route("")]
        public async Task<dynamic> UpdateMetric([FromBody] MetricViewModel param)
        {
            try
            {
                var currentUserId = this.HttpContext.User.Claims.FirstOrDefault(p => p.Type == "UserId").Value;
                if (await _envService.CheckIfUserHasRightToReadEnvAsync(currentUserId, param.EnvId))
                {
                    var validateErrors = ValidateMetric(param);
                    if (validateErrors.Count > 0)
                    {
                        return StatusCode(StatusCodes.Status400BadRequest, new Response { Code = "Error", Messages = validateErrors });
                    }

                    var metric = new Metric
                    {
                        Id = param.Id,
                        Name = param.Name,
                        EnvId = param.EnvId,
                        Description = param.Description,
                        EventName = param.EventName,
                        EventType = param.EventType,
                        CustomEventTrackOption = param.CustomEventTrackOption,
                        MaintainerUserId = param.MaintainerUserId,
                        CustomEventUnit = param.CustomEventUnit,
                        CustomEventSuccessCriteria = param.CustomEventSuccessCriteria,
                        UpdatedAt = DateTime.UtcNow
                    };

                    var newMetric = await _metricService.UpsertItemAsync(metric);
                    param.Id = newMetric.Id;
                    return param;
                }

                return StatusCode(StatusCodes.Status401Unauthorized, new Response { Code = "Error", Message = "Unauthorized" });
            }
            catch (Exception exp)
            {
                _logger.LogError(exp, JsonConvert.SerializeObject(param));

                return StatusCode(StatusCodes.Status500InternalServerError, new Response { Code = "Error", Message = "Internal Error" });
            }
        }

        [HttpGet]
        [Route("")]
        public async Task<dynamic> GetMetrics([FromQuery] int envId, [FromQuery] string searchText, [FromQuery]int page = 0)
        {
            try
            {
                var currentUserId = this.HttpContext.User.Claims.FirstOrDefault(p => p.Type == "UserId").Value;
                if (await _envService.CheckIfUserHasRightToReadEnvAsync(currentUserId, envId))
                {
                    var metrics = await _metricService.GetMetricsAsync(envId, searchText, page, 50);
                    return metrics.Select(m => new MetricViewModel
                    {
                        Id = m.Id,
                        Name = m.Name,
                        EnvId = m.EnvId,
                        Description = m.Description,
                        EventName = m.EventName,
                        EventType = m.EventType,
                        CustomEventTrackOption = m.CustomEventTrackOption,
                        CustomEventUnit = m.CustomEventUnit,
                        CustomEventSuccessCriteria = m.CustomEventSuccessCriteria,
                        MaintainerUserId = m.MaintainerUserId
                    });
                }

                return StatusCode(StatusCodes.Status401Unauthorized, new Response { Code = "Error", Message = "Unauthorized" });
            }
            catch (Exception exp)
            {
                _logger.LogError(exp, JsonConvert.SerializeObject(new { EnvId = envId, searchText = searchText, Page = page }));

                return StatusCode(StatusCodes.Status500InternalServerError, new Response { Code = "Error", Message = "Internal Error" });
            }
        }

        [HttpGet]
        [Route("{envId}/{id}")]
        public async Task<dynamic> GetMetrics(int envId, string id)
        {
            try
            {
                var currentUserId = this.HttpContext.User.Claims.FirstOrDefault(p => p.Type == "UserId").Value;
                if (await _envService.CheckIfUserHasRightToReadEnvAsync(currentUserId, envId))
                {
                    var metrics = await _metricService.GetMetricsByIdsAsync(new List<string> { id });
                    return metrics.Select(m => new MetricViewModel
                    {
                        Id = m.Id,
                        Name = m.Name,
                        EnvId = m.EnvId,
                        Description = m.Description,
                        EventName = m.EventName,
                        EventType = m.EventType,
                        CustomEventTrackOption = m.CustomEventTrackOption,
                        MaintainerUserId = m.MaintainerUserId
                    }).FirstOrDefault();
                }

                return StatusCode(StatusCodes.Status401Unauthorized, new Response { Code = "Error", Message = "Unauthorized" });
            }
            catch (Exception exp)
            {
                _logger.LogError(exp, JsonConvert.SerializeObject(new { EnvId = envId, id = id }));

                return StatusCode(StatusCodes.Status500InternalServerError, new Response { Code = "Error", Message = "Internal Error" });
            }
        }
    }
}
