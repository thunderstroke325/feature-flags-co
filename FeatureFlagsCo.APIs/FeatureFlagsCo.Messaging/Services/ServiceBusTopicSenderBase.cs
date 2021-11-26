using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Text;
using System.Threading.Tasks;
using StackExchange.Redis;

namespace FeatureFlagsCo.Messaging.Services
{
    public abstract class ServiceBusTopicSenderBase
    {
        protected abstract string TopicPath { get; }
        protected ILogger Logger { get; }
        protected IConfiguration Configuration { get; }

        private readonly ServiceBusSender _clientSender;
        private readonly string _engine;

        private readonly IConnectionMultiplexer _redis;

        public ServiceBusTopicSenderBase(
            IConfiguration configuration,
            ILogger logger,
            IConnectionMultiplexer redis)
        {
            Configuration = configuration;
            Logger = logger;
            
            
            _engine = configuration.GetSection("MySettings:BusType").Value;
            if ("azure".Equals(_engine))
            {
                var connectionString = configuration.GetSection("MySettings:ServiceBusConnectionString").Value;
                _clientSender = new ServiceBusClient(connectionString).CreateSender(TopicPath);
            }
            else
            {
                _redis = redis;   
            }

        }
        
        private async Task RedisSendMessageAsync(string message)
        {
            try
            {
                var msg = Encoding.UTF8.GetBytes(message);
                var db = _redis.GetDatabase();
                await db.ListRightPushAsync(TopicPath, msg);
            }
            catch (Exception e)
            {
                Logger.LogError(e.Message);
            }
        }

        private async Task AzureServiceBusSendMessageAsync(string param)
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

        public async Task SendMessageAsync(string param)
        {
            if ("azure".Equals(_engine))
            {
                await AzureServiceBusSendMessageAsync(param);
            }
            else
            {
                await RedisSendMessageAsync(param);
            }
        }
    }
}