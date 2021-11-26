using FeatureFlagsCo.Messaging.ViewModels;
using FeatureFlagsCo.MQ;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using System.Threading.Tasks;
using StackExchange.Redis;

namespace FeatureFlagsCo.Messaging.Services
{
    public class ServiceBusQ1Sender : ServiceBusTopicSenderBase
    {
        protected override string TopicPath
        {
            get { return Configuration.GetSection("MySettings").GetSection("Q1Name").Value; }
        }

        public ServiceBusQ1Sender(IConfiguration configuration, ILogger<ServiceBusQ1Sender> logger, IConnectionMultiplexer redis) : base(
            configuration, logger, redis)
        {
        }
    }
}