using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace FeatureFlagsCo.Messaging.Services
{
    public abstract class ServiceBusTopicSenderBase
    {
        protected abstract string TopicPath { get; }
        protected ILogger Logger { get; }
        protected IConfiguration Configuration { get; }

        private readonly ServiceBusSender _clientSender;

        public ServiceBusTopicSenderBase(
            IConfiguration configuration,
            ILogger logger)
        {
            Configuration = configuration;
            Logger = logger;

            var connectionString = configuration.GetSection("MySettings:ServiceBusConnectionString").Value;
            _clientSender = new ServiceBusClient(connectionString).CreateSender(TopicPath);
        }

        public async Task SendMessageAsync(string param)
        {
            var message = new ServiceBusMessage(param)
            {
                Subject = TopicPath
            };
            message.ApplicationProperties.Add("origin", "standard");

            try
            {
                await _clientSender.SendMessageAsync(message).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                Logger.LogError(e.Message);
            }
        }
    }
}