using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using FeatureFlags.APIs.Models;
using FeatureFlags.Utils.ConventionalDependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace FeatureFlags.APIs.Services
{
    public class SdkWebSocketMessageHandler : ITransientDependency
    {
        private readonly IEnumerable<IMessageHandler> _handlers;
        private readonly ILogger<SdkWebSocketMessageHandler> _logger;

        public SdkWebSocketMessageHandler(
            IEnumerable<IMessageHandler> handlers,
            ILogger<SdkWebSocketMessageHandler> logger)
        {
            _handlers = handlers;
            _logger = logger;
        }

        public async Task HandleAsync(SdkWebSocketMessageContext context)
        {
            var message = context.Message;
            var messageType = message["messageType"]?.ToString();

            var handler = _handlers.FirstOrDefault(handler => handler.MessageType == messageType);
            if (handler == null)
            {
                var exp = new SdkWebSocketMessageHandleException($"no message handler for type '{messageType}'");
                _logger.LogError(exp, exp.Message);
                return;
            }

            try
            {
                await handler.HandleAsync(context);

                _logger.LogDebug(
                    "message {0} from {1} has been handled successfully",
                    message.ToString(Formatting.None), context.Socket.ConnectionId
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "exception errored when handle sdk websocket message {0}", message.ToString());
            }
        }
    }

    public class SdkWebSocketMessageContext
    {
        public SdkWebSocket Socket { get; set; }

        public JObject Message { get; set; }

        public SdkWebSocketMessageContext(SdkWebSocket socket, JObject message)
        {
            Message = message;
            Socket = socket;
        }
    }

    public interface IMessageHandler : ITransientDependency
    {
        string MessageType { get; }

        Task HandleAsync(SdkWebSocketMessageContext context);
    }

    public class DataSyncMessageHandler : IMessageHandler
    {
        public string MessageType => SdkWebSocketMessageTypes.DataSync;

        private readonly SdkSyncDataService _service;
        public DataSyncMessageHandler(SdkSyncDataService service)
        {
            _service = service;
        }

        public async Task HandleAsync(SdkWebSocketMessageContext context)
        {
            var message = context.Message;
            var socket = context.Socket;

            var request = message["data"]?.ToObject<SdkDataSyncRequest>();
            if (request == null)
            {
                throw new ArgumentException($"invalid client message {message}");
            }

            var data = await _service.GetSdkDataAsync(socket, request);

            var response = new SdkWebSocketMessage(SdkWebSocketMessageTypes.DataSync, data);
            await socket.SendAsync(response);
        }
    }
}