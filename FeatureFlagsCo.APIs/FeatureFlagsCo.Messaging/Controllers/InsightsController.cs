using System.Collections.Generic;
using System.Text.Json;
using FeatureFlagsCo.Messaging.Services;
using FeatureFlagsCo.MQ;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using FeatureFlagsCo.Messaging.Models;

namespace FeatureFlagsCo.Messaging.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class InsightsController : ControllerBase
    {
        private readonly ServiceBusQ4Sender _serviceBusSender;
        private readonly ElasticSearchService _elasticSearchService;
        private readonly ILogger<InsightsController> _logger;

        public InsightsController(
            ServiceBusQ4Sender serviceBusSender,
            ElasticSearchService elasticSearchService,
            ILogger<InsightsController> logger)
        {
            _serviceBusSender = serviceBusSender;
            _elasticSearchService = elasticSearchService;
            _logger = logger;
        }

        // Write to Q4
        [HttpPost]
        [Route("")]
        public async Task<dynamic> SendAPIServiceToMQServiceData([FromBody] APIServiceToMQServiceModel param)
        {
            _logger.LogTrace("Insights/SendAPIServiceToMQServiceData");
            if (param == null)
            {
                return StatusCode(StatusCodes.Status400BadRequest, "check your parameter.");
            }
            
            if (param.Message?.Labels != null)
            {
                var keyValues = new Dictionary<string, string>();
                param.Message.Labels.ForEach(label => keyValues.TryAdd(label.LabelName, label.LabelValue));
                var jsonContent = JsonSerializer.Serialize(keyValues);

                var createSuccess = 
                    await _elasticSearchService.CreateDocumentAsync(ElasticSearchIndices.Variation, jsonContent);
                
                // if send to es failed, terminate this request
                if (!createSuccess)
                {
                    const string errMsg = "error occured when create document in elasticsearch, terminate this request.";
                    return StatusCode(StatusCodes.Status500InternalServerError, errMsg);
                }
            }
            
            // send to service bus
            if (param.FFMessage != null)
            {
                var payload = JsonSerializer.Serialize(param.FFMessage);
                await _serviceBusSender.SendMessageAsync(payload);
            }
            
            return StatusCode(StatusCodes.Status200OK, new { Code = "OK", Message = "OK" });
        }
    }
}