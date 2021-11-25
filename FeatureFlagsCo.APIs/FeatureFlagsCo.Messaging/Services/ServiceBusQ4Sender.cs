using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace FeatureFlagsCo.Messaging.Services
{
    public class ServiceBusQ4Sender : ServiceBusTopicSenderBase
    {
        protected override string TopicPath => Configuration.GetSection("MySettings").GetSection("Q4Name").Value;

        public ServiceBusQ4Sender(IConfiguration configuration, ILogger<ServiceBusQ4Sender> logger) 
            : base(configuration, logger)
        {
        }
    }
}