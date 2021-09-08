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
    public class AuditLogSearchController : ControllerBase
    {
        private readonly ILogger<AuditLogSearchController> _logger;
        private readonly IAuditLogSearchService _auditLogSearchService;
        private readonly IOptions<MySettings> _mySettings;

        public AuditLogSearchController(
            ILogger<AuditLogSearchController> logger,
            IAuditLogSearchService auditLogSearchService,
            IOptions<MySettings> mySettings)
        {
            _logger = logger;
            _auditLogSearchService = auditLogSearchService;
            _mySettings = mySettings;
        }


        [HttpGet]
        [Route("GetAllAuditLog")]
        public async Task<JsonResult> GetAllAuditLog(string environmentId, long? startUnixTimeStamp, long? endUnixTimeStamp, int? pageIndex, int? pageSize)
        {
            var r = await _auditLogSearchService.GetAllAuditLogAsync(
                _mySettings.Value.ElasticSearchHost,
                environmentId,
                startUnixTimeStamp ?? (Int64)(DateTime.UtcNow.AddDays(-7).Subtract(new DateTime(1970, 1, 1))).TotalMilliseconds,
                endUnixTimeStamp ?? (Int64)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalMilliseconds,
                pageIndex ?? 0,
                pageSize ?? 20);
            Response.StatusCode = (int)r.Item2;
            return new JsonResult(new
            {
                es = r.Item1
            });
        }

        [HttpGet]
        [Route("GetFeatureFlagAuditLog")]
        public async Task<JsonResult> GetFeatureFlagAuditLog(string featureFlagId, long? startUnixTimeStamp, long? endUnixTimeStamp, int? pageIndex, int? pageSize)
        {
            var r = await _auditLogSearchService.GetFeatureFlagAuditLogAsync(
                _mySettings.Value.ElasticSearchHost,
                featureFlagId,
                startUnixTimeStamp ?? (Int64)(DateTime.UtcNow.AddDays(-7).Subtract(new DateTime(1970, 1, 1))).TotalMilliseconds,
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
