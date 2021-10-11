using System;
using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Text.Json;
using System.Threading.Tasks;

namespace FeatureFlagsCo.MQ
{
    public abstract class ServiceBusTopicSenderBase
    {
        protected abstract string TopicPath { get; }
        private readonly ILogger _logger;
        private readonly ServiceBusClient _client;
        private readonly ServiceBusSender _clientSender;

        public ServiceBusTopicSenderBase(IConfiguration configuration, ILogger<ServiceBusTopicSenderBase> logger)
        {
            _logger = logger;

            var connectionString = configuration.GetConnectionString("ServiceBusConnectionString");
            _client = new ServiceBusClient(connectionString);
            _clientSender = _client.CreateSender(TopicPath);
        }

        public async Task SendMessage(string messagePayload)
        {
            //string messagePayload = JsonSerializer.Serialize(payload);
            ServiceBusMessage message = new ServiceBusMessage(messagePayload);

            try
            {
                await _clientSender.SendMessageAsync(message).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
            }
        }
    }
}
