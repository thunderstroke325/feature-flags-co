using FeatureFlagsCo.Messaging.Services;
using FeatureFlagsCo.Messaging.ViewModels;
using FeatureFlagsCo.MQ;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using System.Threading.Tasks;

namespace FeatureFlagsCo.Messaging.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ExperimentsController: ControllerBase
    {
        private readonly ILogger<ExperimentsController> _logger;
        //private readonly IFeatureFlagMqService _featureFlagsService;
        //private readonly IExperimentStartEndMqService _experimentStartEndMqService;
        //private readonly IExperimentMqService _experimentMqService;
        private readonly ServiceBusQ1Sender _serviceBusQ1Sender;
        private readonly ServiceBusQ5Sender _serviceBusQ5Sender;

        public ExperimentsController(
           ILogger<ExperimentsController> logger,
           ServiceBusQ1Sender serviceBusQ1Sender,
           ServiceBusQ5Sender serviceBusQ5Sender
            )
        {
            _logger = logger;

            _serviceBusQ1Sender = serviceBusQ1Sender;
            _serviceBusQ5Sender = serviceBusQ5Sender;
            //_featureFlagsService = featureFlagsService;
            //_experimentStartEndMqService = experimentStartEndMqService;
            //_experimentMqService = experimentMqService;
        }

        // Write to Q4 for python
        // Not used anymore
        //[HttpPost]
        //[Route("feature-flags")]
        //public async Task<dynamic> SendFeatureFlagData([FromBody] FeatureFlagMessageModel param)
        //{
        //    _logger.LogTrace("Experiments/SendFeatureFlagData");
        //    await _featureFlagsService.SendMessageAsync(param);
        //    return StatusCode(StatusCodes.Status200OK, new { Code = "OK", Message = "OK" });
        //}

        // Write to Q1
        [HttpPost]
        [Route("experiment")]
        public async Task<dynamic> SendExperimentStartEndData([FromBody] ExperimentIterationMessageViewModel param)
        {
            _logger.LogTrace("Experiments/SendExperimentStartEndData");

            string messagePayload = JsonSerializer.Serialize(param);
            await _serviceBusQ1Sender.SendMessageAsync(messagePayload);
            return StatusCode(StatusCodes.Status200OK, new { Code = "OK", Message = "OK" });
        }

        // Write to Q5 for both elasticsearch and python
        [HttpPost]
        [Route("events")]
        public async Task<dynamic> SendEventData([FromBody] ExperimentMessageModel param)
        {
            _logger.LogTrace("Experiments/SendEventData");

            string messagePayload = JsonSerializer.Serialize(param);
            await _serviceBusQ5Sender.SendMessageAsync(messagePayload);
            return StatusCode(StatusCodes.Status200OK, new { Code = "OK", Message = "OK" });
        }
    }
}
