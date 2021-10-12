using FeatureFlagsCo.Messaging.ViewModels;
using FeatureFlagsCo.MQ;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using System.Threading.Tasks;

namespace FeatureFlagsCo.Messaging.Services
{
    public class ServiceBusQ1Sender: ServiceBusTopicSenderBase
    {
        protected override string TopicPath { get { return "q1"; } }

        public ServiceBusQ1Sender(IConfiguration configuration, ILogger<ServiceBusQ1Sender> logger) : base(configuration, logger)
        {
        }
    }
}
