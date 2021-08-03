using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using FeatureFlags.APIs.Authentication;
using FeatureFlags.APIs.Models;
using FeatureFlags.APIs.Repositories;
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


        //private readonly ILaunchDarklyService _ldService;

        public WeatherForecastController(ILogger<WeatherForecastController> logger,
            IDistributedCache redisCache)
            //ILaunchDarklyService ldService)
        {
            _logger = logger;
            _redisCache = redisCache;
            //_ldService = ldService;
        }

        //[HttpGet]
        //public IEnumerable<WeatherForecast> Get()
        //{
        //    var a= _redisCache.GetString("a");
        //    _redisCache.SetString("a", "cccefeg");
        //    a = _redisCache.GetString("a");
        //    var rng = new Random();
        //    return Enumerable.Range(1, 5).Select(index => new WeatherForecast
        //    {
        //        Date = DateTime.Now.AddDays(index),
        //        TemperatureC = rng.Next(-20, 55),
        //        Summary = Summaries[rng.Next(Summaries.Length)]
        //    })
        //    .ToArray();
        //}

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
            Response.StatusCode = 200;
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


        //[HttpGet]
        //[Route("ldtest")]
        //public bool LDTest()
        //{
        //    return _ldService.GetVariation("new-feature");
        //}

        [HttpGet]
        [Route("probe")]
        public IActionResult Probe()
        {
            return Ok();
        }
    }
}
