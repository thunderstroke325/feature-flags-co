using FeatureFlagsCo.MQ;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Threading.Tasks;

namespace FeatureFlagsCo.Messaging.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class InsightsController : ControllerBase
    {
        ILogger<InsightsController> _logger;
        private readonly IInsighstMqService _insightsService;

        public InsightsController(
           ILogger<InsightsController> logger,
           IInsighstMqService insightsService)
        {
            _logger = logger;
            
            _insightsService = insightsService;
        }

        // Write to Q4 for elasticsearch
        [HttpPost]
        [Route("")]
        public async Task<dynamic> SendInsightData([FromBody] MessageModel param)
        {
            _logger.LogTrace("Insights/SendInsightData");
            await _insightsService.SendMessageAsync(param);
            return StatusCode(StatusCodes.Status200OK, new { Code = "OK", Message = "OK" });
        }
    }
}
