using System;
using System.Threading.Tasks;
using FeatureFlags.APIs.Models;
using FeatureFlags.Utils.ConventionalDependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;

namespace FeatureFlags.APIs.Services
{
    public class SdkWebSocketService : ITransientDependency
    {
        private readonly ILogger<SdkWebSocketService> _logger;
        private readonly SdkWebSocketConnectionManager _connectionManager;
        private readonly SdkWebSocketMessageHandler _messageHandler;

        public SdkWebSocketService(
            ILogger<SdkWebSocketService> logger,
            SdkWebSocketConnectionManager connectionManager,
            SdkWebSocketMessageHandler messageHandler)
        {
            _logger = logger;
            _connectionManager = connectionManager;
            _messageHandler = messageHandler;
        }

        public async Task OnConnectedAsync(SdkWebSocket socket)
        {
            _logger.LogInformation("socket {0} connected", socket.ToString());

            _connectionManager.RegisterSocket(socket);

            await Task.CompletedTask;
        }

        public async Task OnDisconnectedAsync(SdkWebSocket socket)
        {
            _logger.LogInformation("socket {0} disconnected", socket.ToString());
            await _connectionManager.UnregisterSocket(socket);
        }

        public async Task OnMessageAsync(
            IServiceProvider serviceProvider,
            SdkWebSocket socket,
            string message)
        {
            JObject json;

            try
            {
                json = JObject.Parse(message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "socket {0} sent invalid message {1}", socket.ToString(), message);
                return;
            }

            var context = new SdkWebSocketMessageContext(serviceProvider, socket, json);

            await _messageHandler.HandleAsync(context);
        }
    }
}