using FeatureFlagsCo.Messaging.Services;
using FeatureFlagsCo.Messaging.ViewModels;
using FeatureFlagsCo.MQ;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using System.Threading.Tasks;
using FeatureFlagsCo.MQ.ElasticSearch;

namespace FeatureFlagsCo.Messaging.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ExperimentsController: ControllerBase
    {
        private readonly ILogger<ExperimentsController> _logger;
        private readonly ServiceBusQ1Sender _serviceBusQ1Sender;
        private readonly ServiceBusQ5Sender _serviceBusQ5Sender;
        private readonly ElasticSearchService _elasticSearch;

        public ExperimentsController(
           ILogger<ExperimentsController> logger,
           ServiceBusQ1Sender serviceBusQ1Sender,
           ServiceBusQ5Sender serviceBusQ5Sender, 
           ElasticSearchService elasticSearch)
        {
            _logger = logger;
            _serviceBusQ1Sender = serviceBusQ1Sender;
            _serviceBusQ5Sender = serviceBusQ5Sender;
            _elasticSearch = elasticSearch;
        }

        // Write to Q1
        [HttpPost]
        [Route("experiment")]
        public async Task<dynamic> SendExperimentStartEndData([FromBody] ExperimentIterationMessageViewModel param)
        {
            _logger.LogTrace("Experiments/SendExperimentStartEndData");

            // send message to queue
            var messagePayload = JsonSerializer.Serialize(param);
            await _serviceBusQ1Sender.SendMessageAsync(messagePayload);
            
            // send message to es
            await _elasticSearch.IndexDocumentAsync(param, ElasticSearchIndices.Experiment);
            
            return StatusCode(StatusCodes.Status200OK, new { Code = "OK", Message = "OK" });
        }

        // Write to Q5
        [HttpPost]
        [Route("events")]
        public async Task<dynamic> SendEventData([FromBody] ExperimentMessageModel param)
        {
            _logger.LogTrace("Experiments/SendEventData");

            // send message to queue
            var messagePayload = JsonSerializer.Serialize(param);
            await _serviceBusQ5Sender.SendMessageAsync(messagePayload);
            
            // send message to es
            await _elasticSearch.IndexDocumentAsync(param, ElasticSearchIndices.Experiment);
            
            return StatusCode(StatusCodes.Status200OK, new { Code = "OK", Message = "OK" });
        }
    }
}
