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
        private readonly SdkSyncDataService _syncDataService;
        private readonly FeatureFlagV2Service _flagService;

        public SdkWebSocketService(
            ILogger<SdkWebSocketService> logger,
            SdkWebSocketConnectionManager connectionManager,
            SdkWebSocketMessageHandler messageHandler,
            SdkSyncDataService syncDataService, 
            FeatureFlagV2Service flagService)
        {
            _logger = logger;
            _connectionManager = connectionManager;
            _messageHandler = messageHandler;
            _syncDataService = syncDataService;
            _flagService = flagService;
        }

        public async Task OnConnectedAsync(SdkWebSocket socket)
        {
            _logger.LogInformation("socket {0} connected", socket.ToString());

            _connectionManager.RegisterSocket(socket);

            await Task.CompletedTask;
        }

        public async Task OnDisconnectedAsync(SdkWebSocket socket)
        {
            await _connectionManager.UnregisterSocket(socket);
            
            _logger.LogInformation("socket {0} disconnected", socket.ToString());
        }

        public async Task OnMessageAsync(SdkWebSocket socket, string message)
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

            var context = new SdkWebSocketMessageContext(socket, json);

            await _messageHandler.HandleAsync(context);
        }

        public async Task OnFeatureFlagChangeAsync(string featureFlagId)
        {
            var flag = await _flagService.GetAsync(featureFlagId);
            if (flag == null)
            {
                return;
            }
            
            var sockets = _connectionManager.GetSockets(flag.EnvironmentId);
            foreach (var socket in sockets)
            {
                try
                {
                    var data = await _syncDataService.GetSdkDataAsync(SdkDataSyncTypes.Patch, flag, socket);

                    var message = new SdkWebSocketMessage(SdkWebSocketMessageTypes.DataSync, data);

                    await socket.SendAsync(message);
                    _logger.LogInformation("patch data for feature flag {0} handled", featureFlagId);
                }
                catch (Exception ex)
                {
                    var error = new SdkWebSocketMessageHandleException(
                        $"sync patch data for featureFlag {featureFlagId} error, socket {socket}", ex
                    );

                    _logger.LogError(error, error.Message);
                }
            }
        }
    }
}