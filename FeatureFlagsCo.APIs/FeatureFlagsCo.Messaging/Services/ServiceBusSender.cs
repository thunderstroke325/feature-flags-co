using Azure.Messaging.ServiceBus;
using FeatureFlagsCo.MQ;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace FeatureFlagsCo.Messaging.Services
{
    public class ServiceBusSender
    {
        private readonly ServiceBusClient _client;
        private readonly Azure.Messaging.ServiceBus.ServiceBusSender _clientSender;

        public ServiceBusSender(string connectionString, string queueName)
        {
            _client = new ServiceBusClient(connectionString);
            _clientSender = _client.CreateSender(queueName);
        }

        public async Task SendMessageAsync(MessageModel model)
        {
            string messagePayload = JsonSerializer.Serialize(model);
            ServiceBusMessage message = new ServiceBusMessage(messagePayload);
            await _clientSender.SendMessageAsync(message).ConfigureAwait(false);
        }

        public async Task SendAPIServiceToMQServiceData(APIServiceToMQServiceModel param)
        {
            string messagePayload = JsonSerializer.Serialize(param);
            ServiceBusMessage message = new ServiceBusMessage(messagePayload);
            await _clientSender.SendMessageAsync(message).ConfigureAwait(false);
        }
    }
}
