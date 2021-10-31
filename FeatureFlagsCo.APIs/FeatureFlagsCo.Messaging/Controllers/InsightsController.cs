
using FeatureFlagsCo.Messaging.Services;
using FeatureFlagsCo.MQ;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text.Json;
using System.Threading.Tasks;

namespace FeatureFlagsCo.Messaging.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class InsightsController : ControllerBase
    {
        ILogger<InsightsController> _logger;
        //private readonly IInsighstMqService _insightsService;
        private readonly ServiceBusQ4Sender _serviceBusSender;

        public InsightsController(
           ILogger<InsightsController> logger,
           ServiceBusQ4Sender serviceBusSender)
        {
            _logger = logger;
            _serviceBusSender = serviceBusSender;
        }

        // Write to Q4 for elasticsearch
        //[HttpPost]
        //[Route("")]
        //public async Task<dynamic> SendInsightData([FromBody] MessageModel param)
        //{
        //    _logger.LogTrace("Insights/SendInsightData");
        //    await _insightsService.SendMessageAsync(param);
        //    return StatusCode(StatusCodes.Status200OK, new { Code = "OK", Message = "OK" });
        //}

        [HttpPost]
        [Route("")]
        public async Task<dynamic> SendAPIServiceToMQServiceData([FromBody] APIServiceToMQServiceModel param)
        {
            _logger.LogTrace("Insights/SendAPIServiceToMQServiceData");
            // TODO deal with es later
            string messagePayload = JsonSerializer.Serialize(param.FFMessage);
            await _serviceBusSender.SendMessageAsync(messagePayload);
            return StatusCode(StatusCodes.Status200OK, new { Code = "OK", Message = "OK" });
        }
    }
}
