using Azure.Messaging.ServiceBus;
using FeatureFlagsCo.Messaging.ViewModels;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;

namespace FeatureFlagsCo.Messaging.Services
{
    public class ServiceBusQ1Receiver : ServiceBusReceiverBase
    {
        protected override string TopicPath { get { return "q1"; } }

        private ILogger<ServiceBusQ1Receiver> _logger;
        private readonly IExperimentStartEndMqService _experimentStartEndmqService;

        public ServiceBusQ1Receiver(
            IConfiguration configuration,
            IExperimentStartEndMqService experimentStartEndmqService,
            ILogger<ServiceBusQ1Receiver> logger
            ) : base (configuration)
        {
            _logger = logger;
            _experimentStartEndmqService = experimentStartEndmqService;
        }

        public override Task Processor_ProcessErrorAsync(ProcessErrorEventArgs arg)
        {
            _logger.LogError(arg.Exception, "ServiceBusQ1Receiver apitomq Error");
            return Task.CompletedTask;
        }

        public override async Task Processor_ProcessMessageAsync(ProcessMessageEventArgs args)
        {
            string body = args.Message.Body.ToString();
            Console.WriteLine($"Q1 Received: {body}");
            var data = JsonConvert.DeserializeObject<ExperimentIterationMessageViewModel>(body);
            if(data != null)
            {
                var writeMessageResult = await _experimentStartEndmqService.SendMessageAsync(data);
                if (writeMessageResult == false)
                    return;
            }
            

            await args.CompleteMessageAsync(args.Message);
        }
    }
}
