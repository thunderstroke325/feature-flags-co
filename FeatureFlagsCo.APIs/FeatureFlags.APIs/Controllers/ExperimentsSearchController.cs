using FeatureFlags.APIs.Models;
using FeatureFlags.APIs.Repositories;
using FeatureFlags.APIs.Services;
using FeatureFlags.APIs.ViewModels;
using FeatureFlags.APIs.ViewModels.ExperimentsDataReceiver;
using FeatureFlagsCo.FeatureInsights;
using FeatureFlagsCo.MQ;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

namespace FeatureFlags.APIs.Controllers
{
    //[Authorize(Roles = UserRoles.Admin)]
    //[Authorize]
    [ApiController]
    [Route("[controller]")]
    public class ExperimentsSearchController : ControllerBase
    {
        private readonly ILogger<ExperimentsSearchController> _logger;
        private readonly IExperimentationService _experimentsService;
        private readonly IOptions<MySettings> _mySettings;

        public ExperimentsSearchController(
            ILogger<ExperimentsSearchController> logger,
            IExperimentationService experimentsService,
            IOptions<MySettings> mySettings)
        {
            _logger = logger;
            _experimentsService = experimentsService;
            _mySettings = mySettings;
    }


        [HttpGet]
        [Route("RawData")]
        public async Task<JsonResult> GetList(string secret, long? startUnixTimeStamp, long? endUnixTimeStamp, int? pageIndex, int? pageSize)
        {
            var ffIds = FeatureFlagKeyExtension.GetEnvIdsByEnvKey(secret);
            var r = await _experimentsService.GetListAsync(
                _mySettings.Value.ElasticSearchHost,
                ffIds.EnvId,
                startUnixTimeStamp ?? (Int64)(DateTime.UtcNow.AddDays(-1).Subtract(new DateTime(1970, 1, 1))).TotalMilliseconds,
                endUnixTimeStamp ?? (Int64)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalMilliseconds,
                pageIndex ?? 0,
                pageSize ?? 20);
            Response.StatusCode = (int)r.Item2;
            return new JsonResult(new
            {
                es = r.Item1
            });
        }
    }
}
