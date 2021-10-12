using Azure.Messaging.ServiceBus;
using FeatureFlagsCo.Messaging.ViewModels;
using FeatureFlagsCo.MQ;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;

namespace FeatureFlagsCo.Messaging.Services
{
    public class ServiceBusQ5Receiver : ServiceBusReceiverBase
    {
        protected override string TopicPath { get { return "q5"; } }

        private ILogger<ServiceBusQ5Receiver> _logger;
        private readonly IExperimentMqService _experimentstRabbitMqService;

        public ServiceBusQ5Receiver(
            IConfiguration configuration,
            IExperimentMqService experimentstRabbitMqService,
            ILogger<ServiceBusQ5Receiver> logger
            ) : base (configuration)
        {
            _logger = logger;
            _experimentstRabbitMqService = experimentstRabbitMqService;
        }

        public override Task Processor_ProcessErrorAsync(ProcessErrorEventArgs arg)
        {
            _logger.LogError(arg.Exception, "ServiceBusQ1Receiver apitomq Error");
            return Task.CompletedTask;
        }

        public override async Task Processor_ProcessMessageAsync(ProcessMessageEventArgs args)
        {
            string body = args.Message.Body.ToString();
            Console.WriteLine($"Q5 Received: {body}");
            var data = JsonConvert.DeserializeObject<ExperimentMessageModel>(body);

            if(data != null)
            {
                var writeMessageResult = await _experimentstRabbitMqService.SendMessageAsync(data);
                if (writeMessageResult == false)
                    return;
            }
            
            await args.CompleteMessageAsync(args.Message); 
        }
    }
}
