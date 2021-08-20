using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using FeatureFlags.APIs.Authentication;
using FeatureFlags.APIs.Models;
using FeatureFlags.APIs.Repositories;
using FeatureFlags.APIs.Services;
using FeatureFlags.APIs.ViewModels;
using Microsoft.ApplicationInsights;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace FeatureFlags.APIs.Controllers
{
    //[Authorize(Roles = UserRoles.Admin)]
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        private static readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        private readonly ILogger<WeatherForecastController> _logger;
        private readonly IDistributedCache _redisCache;
        private readonly IInsighstRabbitMqService _rabbitmqInsightsService;
        private readonly INoSqlService _noSqlService;

        //private readonly ILaunchDarklyService _ldService;

        public WeatherForecastController(ILogger<WeatherForecastController> logger,
            IDistributedCache redisCache,
            IInsighstRabbitMqService rabbitmqInsightsService,
            INoSqlService noSqlService)
            //ILaunchDarklyService ldService)
        {
            _logger = logger;
            _redisCache = redisCache;
            _rabbitmqInsightsService = rabbitmqInsightsService;
            _noSqlService = noSqlService;
        }

        [HttpGet]
        [Route("CreateMockMongoFeatureFlag")]
        public async Task<JsonResult> CreateMockMongoFeatureFlag()
        {
            var guid = Guid.NewGuid().ToString();
            FeatureFlag newFF = null;
            try
            {
                newFF = await _noSqlService.CreateFeatureFlagAsync(new CreateFeatureFlagViewModel
                {
                    CreatorUserId = "",
                    EnvironmentId = -1,
                    KeyName = guid,
                    Name = guid,
                    Status = FeatureFlagStatutEnum.Enabled.ToString(),
                    Id = FeatureFlagKeyExtension.GetFeatureFlagId(guid, "-1")
                }, "", -1, -1);
            }
            catch(Exception exp)
            {

            }
            return new JsonResult(newFF);
        }

        [HttpPost]
        [Route("redistest")]
        public string RedisTest([FromBody] GetUserVariationResultParam param)
        {
            var date = Guid.NewGuid().ToString() + DateTime.UtcNow.ToString();
            var serializedParam = JsonConvert.SerializeObject(param);
            var customizedTraceProperties = new Dictionary<string, object>()
            {
                ["envId"] = param.FeatureFlagKeyName,
                ["accountId"] = param.FeatureFlagKeyName,
                ["projectId"] = param.FeatureFlagKeyName,
                ["userKeyName"] = param.FeatureFlagKeyName,
                ["serializedParam"] = serializedParam
            };
            _redisCache.SetString(date, serializedParam);
            using (_logger.BeginScope(customizedTraceProperties))
            {
                _logger.LogInformation("variation-wr-request");
                //_logger.LogWarning("variation-wr-request");
            }
            return _redisCache.GetString(date);
        }

        [HttpPost]
        [Route("throwexception")]
        public string ThrowException([FromBody] GetUserVariationResultParam param)
        {
            throw new Exception("ThrowException test");
        }

        [HttpGet]
        [Route("ReturnTest200")]
        public JsonResult ReturnTest200()
        {
            var date = Guid.NewGuid().ToString();
            var customizedTraceProperties = new Dictionary<string, object>()
            {
                ["envId"] = "param.FeatureFlagKeyName",
                ["accountId"] = "param.FeatureFlagKeyName",
                ["projectId"] = "param.FeatureFlagKeyName",
                ["userKeyName"] = "param.FeatureFlagKeyName",
                ["serializedParam"] = "serializedParam"
            };
            _redisCache.SetString(date, JsonConvert.SerializeObject(customizedTraceProperties));
            _redisCache.GetString(date);
            Response.StatusCode = 200;
            _rabbitmqInsightsService.SendMessage(new FeatureFlagsCo.RabbitMqModels.MessageModel
            {
                Labels = new List<FeatureFlagsCo.RabbitMqModels.MessageLabel>()
                 {
                     new FeatureFlagsCo.RabbitMqModels.MessageLabel{ LabelName = "email", LabelValue = "hu-beau@outlook.com"},
                     new FeatureFlagsCo.RabbitMqModels.MessageLabel{ LabelName = "timestamp", LabelValue = DateTime.UtcNow.ToString()}
                 },
                Message = "Very very Very very Very very Very very Very very Very very long message.",
                SendDateTime = DateTime.UtcNow
            });
            return new JsonResult(new VariationOption());
        }

        [HttpGet]
        [Route("ReturnTest501")]
        public JsonResult ReturnTest501()
        {
            Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            return new JsonResult(new VariationOption());
        }

        [HttpGet]
        [Route("redistest2")]
        public string RedisTest2()
        {
            return "true";
        }


        [HttpGet]
        [Route("probe")]
        public IActionResult Probe()
        {
            return Ok();
        }
    }
}
