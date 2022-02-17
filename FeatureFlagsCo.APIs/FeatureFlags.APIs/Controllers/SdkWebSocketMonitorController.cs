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
            var connections = _connectionManager.GetAll().ToList();

            var infos = connections.Select(connection => connection?.ToString() ?? "null").ToList();

            var insights = new
            {
                total = connections.Count,
                hasNull = connections.Any(x => x == null),
                data = infos
            };
            
            return Ok(insights);
        }
    }
}