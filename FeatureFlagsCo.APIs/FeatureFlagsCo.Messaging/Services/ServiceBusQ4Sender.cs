using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace FeatureFlagsCo.Messaging.Services
{
    public class ServiceBusQ4Sender : ServiceBusTopicSenderBase
    {
        protected override string TopicPath => Configuration.GetSection("MySettings").GetSection("Q4Name").Value;

        public ServiceBusQ4Sender(IConfiguration configuration, ILogger<ServiceBusQ4Sender> logger, IConnectionMultiplexer redis) 
            : base(configuration, logger, redis)
        {
        }
    }
}