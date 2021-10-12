
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
    public class HealthController : ControllerBase
    {
        public HealthController(
            ServiceBusQ1Sender serviceBusQ1Sender,
            ServiceBusQ5Sender serviceBusQ5Sender,
            ServiceBusQ4Sender serviceBusSender)
        {
        }


        [HttpGet]
        [Route("")]
        public IActionResult Get()
        {
            return Ok();
        }
    }
}
