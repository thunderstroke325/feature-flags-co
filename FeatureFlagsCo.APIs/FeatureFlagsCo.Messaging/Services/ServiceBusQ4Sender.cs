using FeatureFlagsCo.MQ;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using System.Threading.Tasks;

namespace FeatureFlagsCo.Messaging.Services
{
    public class ServiceBusQ4Sender: ServiceBusTopicSenderBase
    {
        protected override string TopicPath
        {
            get { return Configuration.GetSection("MySettings").GetSection("Q4Name").Value; }
        }

        public ServiceBusQ4Sender(IConfiguration configuration, ILogger<ServiceBusQ4Sender> logger) : base(configuration, logger)
        {
        }

        //public async Task SendMessageAsync(MessageModel model)
        //{
        //    string messagePayload = JsonSerializer.Serialize(model);
        //    ServiceBusMessage message = new ServiceBusMessage(messagePayload);
        //    await _clientSender.SendMessageAsync(message).ConfigureAwait(false);
        //}

        public async Task SendAPIServiceToMQServiceData(APIServiceToMQServiceModel param)
        {
            string messagePayload = JsonSerializer.Serialize(param);
            await base.SendMessageAsync(messagePayload);
        }
    }
}
