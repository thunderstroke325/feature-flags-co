using System;
using System.Threading.Tasks;
using FeatureFlags.APIs.Authentication;
using FeatureFlags.APIs.ViewModels;
using FeatureFlags.APIs.ViewModels.FeatureFlagsViewModels;
using FeatureFlagsCo.FeatureInsights;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FeatureFlags.APIs.Controllers
{
    [Authorize]
    [ApiController]
    [Route("[controller]")]
    public class FeatureFlagUsageController : ControllerBase
    {
        private readonly ILogger<FeatureFlagUsageController> _logger;
        private readonly IFeatureFlagsUsageService _ffUsageService;
        private readonly IOptions<MySettings> _mySettings;

        public FeatureFlagUsageController(
            ILogger<FeatureFlagUsageController> logger,
            IFeatureFlagsUsageService ffUsageService,
            IOptions<MySettings> mySettings)
        {
            _logger = logger;
            _mySettings = mySettings;
            _ffUsageService = ffUsageService;
        }

        [HttpGet]
        [Route("GetMultiOptionFeatureFlagUsageData")]
        [AllowAnonymous]
        public async Task<dynamic> GetMultiOptionFeatureFlagUsageData(string featureFlagId, string chartQueryTimeSpan)
        {
            try
            {
                var returnModel = new FeatureFlagUsageViewModel();

                DateTime startUTCDateTime = DateTime.UtcNow.AddDays(-7), endUtcDateTime = DateTime.UtcNow;
                int interval = 7;
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
                return StatusCode(StatusCodes.Status500InternalServerError, new Response { Code = "Error", Message = "Bad Request" });
            }
        }

    }
}
