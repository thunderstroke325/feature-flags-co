using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using FeatureFlags.APIs.Models;
using FeatureFlags.APIs.Repositories;
using FeatureFlags.APIs.Services;
using FeatureFlags.APIs.ViewModels;
using FeatureFlags.APIs.ViewModels.FeatureFlagsViewModels;
using FeatureFlagsCo.FeatureInsights;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
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
        private readonly IFeatureFlagsUsageService _ffUsageService;
        private readonly IOptions<MySettings> _mySettings;
        private readonly IEnvironmentService _envService;

        public FeatureFlagUsageController(
            IAppInsightsService appInsightsService,
            INoSqlService cosmosDbService,
            ILogger<FeatureFlagUsageController> logger,
             IFeatureFlagsUsageService ffUsageService,
             IOptions<MySettings> mySettings,
             IEnvironmentService envService)
        {
            _appInsightsService = appInsightsService;
            _cosmosDbService = cosmosDbService;
            _logger = logger;
            _mySettings = mySettings;
            _ffUsageService = ffUsageService;
            _envService = envService;
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
            //returnModel.TotalUsers = await _cosmosDbService.TrueFalseStatusGetFeatureFlagTotalUsersAsync(featureFlagId);
            //returnModel.HitUsers = await _cosmosDbService.TrueFalseStatusGetFeatureFlagHitUsersAsync(featureFlagId);
            //returnModel.ChartData = _appInsightsService.GetFFUsageChartData(featureFlagId, chartQueryTimeSpan);
            return returnModel;
        }

        [HttpGet]
        [Route("GetMultiOptionFeatureFlagUsageData")]
        [AllowAnonymous]
        public async Task<JsonResult> GetMultiOptionFeatureFlagUsageData(string featureFlagId, string chartQueryTimeSpan)
        {
            int envId = FeatureFlagKeyExtension.GetEnvIdByFeautreFlagId(featureFlagId);
            //var currentUserId = this.HttpContext.User.Claims.FirstOrDefault(p => p.Type == "UserId").Value;
            //if ((await _envService.CheckIfUserHasRightToReadEnvAsync(currentUserId, envId)) == false) {
            //    Response.StatusCode = (int)HttpStatusCode.Unauthorized;
            //    return new JsonResult("Unauthorized");
            //}
            try
            {
                var returnModel = new FeatureFlagUsageViewModel();
                //returnModel.ChartData = _appInsightsService.GetFFUsageChartData(featureFlagId, chartQueryTimeSpan);
                //returnModel.UserDistribution = _appInsightsService.GetFFUserDistribution(featureFlagId, chartQueryTimeSpan);

                DateTime startUTCDateTime = DateTime.UtcNow.AddDays(-7), endUtcDateTime = DateTime.UtcNow;
                int interval = 10;
                if (chartQueryTimeSpan == FeatureFlagUsageChartQueryTimeSpanEnum.P1D.ToString())
                    startUTCDateTime = endUtcDateTime.AddDays(-1);
                if (chartQueryTimeSpan == FeatureFlagUsageChartQueryTimeSpanEnum.P7D.ToString())
                    startUTCDateTime = endUtcDateTime.AddDays(-7);
                if (chartQueryTimeSpan == FeatureFlagUsageChartQueryTimeSpanEnum.PT2H.ToString())
                    startUTCDateTime = endUtcDateTime.AddHours(-2);
                if (chartQueryTimeSpan == FeatureFlagUsageChartQueryTimeSpanEnum.PT30M.ToString())
                    startUTCDateTime = endUtcDateTime.AddMinutes(-30);

                returnModel.ChartData = await _ffUsageService.GetLinearUsageCountByTimeRangeAsync(_mySettings.Value.ElasticSearchHost,
                                                                 "ffvariationrequestindex",
                                                                 featureFlagId,
                                                                 startUTCDateTime,
                                                                 endUtcDateTime,
                                                                 interval);

                returnModel.UserByVariationValue = await _ffUsageService.GetFFVariationUserCountAsync(_mySettings.Value.ElasticSearchHost,
                                                                   "ffvariationrequestindex",
                                                                   featureFlagId);
                return new JsonResult(returnModel);
            }
            catch (Exception exp)
            {
                _logger.LogError(exp, $"Get /FeatureFlagUsage/GetMultiOptionFeatureFlagUsageData ; featureFlagId: {featureFlagId}, chartQueryTimeSpan: {chartQueryTimeSpan} ");
                Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                return new JsonResult(exp.Message);
            }
        }

    }
}
