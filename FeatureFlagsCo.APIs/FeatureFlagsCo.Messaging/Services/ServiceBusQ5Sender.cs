using FeatureFlagsCo.MQ;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using System.Threading.Tasks;

namespace FeatureFlagsCo.Messaging.Services
{
    public class ServiceBusQ5Sender: ServiceBusTopicSenderBase
    {
        protected override string TopicPath { get { return "q5"; } }

        public ServiceBusQ5Sender(IConfiguration configuration, ILogger<ServiceBusQ5Sender> logger) : base(configuration, logger)
        {
        }
    }
}
