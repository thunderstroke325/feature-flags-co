using System.Linq;
using FeatureFlags.APIs.Controllers.Base;
using FeatureFlags.APIs.Services;
using Microsoft.AspNetCore.Mvc;

namespace FeatureFlags.APIs.Controllers
{
    public class SdkWebSocketMonitorController : ApiControllerBase
    {
        private readonly SdkWebSocketConnectionManager _connectionManager;

        public SdkWebSocketMonitorController(SdkWebSocketConnectionManager connectionManager)
        {
            _connectionManager = connectionManager;
        }
        
        [HttpGet("all-connection")]
        public IActionResult GetAll()
        {
            var infos = _connectionManager.GetAll().Select(x => x.ToString());
            
            return Ok(infos);
        }
    }
}