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
            var connections = _connectionManager.GetAll()
                .OrderBy(x => x.ConnectAt)
                .Select(socket => new
                {
                    socket.ConnectionId, 
                    socket.EnvSecret, 
                    socket.SdkType, 
                    socket.ConnectAt,
                    socket.DisConnectAt, 
                    socket.User,
                })
                .ToList();

            var insights = new
            {
                total = connections.Count,
                hasNull = connections.Any(x => x == null),
                data = connections
            };
            
            return Ok(insights);
        }
    }
}