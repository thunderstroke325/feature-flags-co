using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using FeatureFlags.APIs.Models;
using FeatureFlags.APIs.Repositories;
using FeatureFlags.APIs.Services;
using FeatureFlags.APIs.ViewModels.FeatureFlagsViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace FeatureFlags.APIs.Controllers
{
    //[Authorize(Roles = UserRoles.Admin)]
    [Authorize]
    [ApiController]
    [Route("[controller]")]
    public class FeatureFlagUsageController : ControllerBase
    {
        private readonly IAppInsightsService _appInsightsService;
        private readonly INoSqlService _cosmosDbService;
        private readonly ILogger<FeatureFlagUsageController> _logger;

        public FeatureFlagUsageController(
            IAppInsightsService appInsightsService,
            INoSqlService cosmosDbService,
            ILogger<FeatureFlagUsageController> logger)
        {
            _appInsightsService = appInsightsService;
            _cosmosDbService = cosmosDbService;
            _logger = logger;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="featureFlagId"></param>
        /// <param name="chartRange">7天=P7D, 24小时=P1D, 2小时=PT2H, 30分钟=PT30M</param>
        /// <returns></returns>
        [HttpGet]
        [Route("GetFeatureFlagUsageData")]
        public async Task<FeatureFlagUsageViewModel> GetFeatureFlagUsageData(string featureFlagId, string chartQueryTimeSpan)
        {
            var returnModel = new FeatureFlagUsageViewModel();
            returnModel.TotalUsers = await _cosmosDbService.TrueFalseStatusGetFeatureFlagTotalUsersAsync(featureFlagId);
            returnModel.HitUsers = await _cosmosDbService.TrueFalseStatusGetFeatureFlagHitUsersAsync(featureFlagId);
            returnModel.ChartData = _appInsightsService.GetFFUsageChartData(featureFlagId, chartQueryTimeSpan);
            return returnModel;
        }

        [HttpGet]
        [Route("GetMultiOptionFeatureFlagUsageData")]
        public JsonResult GetMultiOptionFeatureFlagUsageData(string featureFlagId, string chartQueryTimeSpan)
        {
            try
            {
                var returnModel = new FeatureFlagUsageViewModel();
                returnModel.ChartData = _appInsightsService.GetFFUsageChartData(featureFlagId, chartQueryTimeSpan);
                returnModel.UserDistribution = _appInsightsService.GetFFUserDistribution(featureFlagId, chartQueryTimeSpan);
                return new JsonResult(returnModel);
            }
            catch(Exception exp)
            {
                _logger.LogError(exp, $"Get /FeatureFlagUsage/GetMultiOptionFeatureFlagUsageData ; featureFlagId: {featureFlagId}, chartQueryTimeSpan: {chartQueryTimeSpan} ");
                Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                return new JsonResult(exp.Message);
            }
        }

    }
}
