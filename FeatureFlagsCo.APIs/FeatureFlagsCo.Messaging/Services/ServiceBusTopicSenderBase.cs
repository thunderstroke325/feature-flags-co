using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FeatureFlagsCo.Messaging.Services
{
    public abstract class ServiceBusTopicSenderBase
    {
        protected abstract string TopicPath { get; }
        private readonly ILogger _logger;
        private readonly ServiceBusClient _client;
        private readonly ServiceBusSender _clientSender;
        protected IConfiguration Configuration { get; set; }

        public ServiceBusTopicSenderBase(IConfiguration configuration, ILogger<ServiceBusTopicSenderBase> logger)
        {
            _logger = logger;
            Configuration = configuration;
            var connectionString = Configuration.GetSection("MySettings").GetSection("ServiceBusConnectionString").Value;
            _client = new ServiceBusClient(connectionString);
            _clientSender = _client.CreateSender(TopicPath);
        }

        //public async Task SendMessageAsync(MessageModel model)
        //{
        //    string messagePayload = JsonSerializer.Serialize(model);
        //    ServiceBusMessage message = new ServiceBusMessage(messagePayload);
        //    await _clientSender.SendMessageAsync(message).ConfigureAwait(false);
        //}

        public async Task SendMessageAsync(string param)
        {
            ServiceBusMessage message = new ServiceBusMessage(param);
            message.Subject = TopicPath;
            message.ApplicationProperties.Add("origin", "standard");
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
