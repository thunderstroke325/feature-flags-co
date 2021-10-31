using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;
using FeatureFlagsCo.Messaging.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace FeatureFlagsCo.Messaging.Services
{
    public class ServiceBusQ3Receiver : ServiceBusReceiverBase
    {
        protected override string TopicPath
        {
            get { return Configuration.GetSection("MySettings").GetSection("Q3Name").Value; }
        }

        private readonly ILogger<ServiceBusQ3Receiver> _logger;

        private readonly ExperimentsService _experimentsService;

        public ServiceBusQ3Receiver(IConfiguration configuration,
            ExperimentsService experimentsService,
            ILogger<ServiceBusQ3Receiver> logger) : base(configuration)
        {
            _logger = logger;
            _experimentsService = experimentsService;
            using (_logger.BeginScope(new Dictionary<string, string>()
                {
                    ["topic"] = TopicPath,
                    ["subscription"] = "standard"
                }
            ))
            {
                _logger.LogTrace("RECEIVER START");
            }
        }

        public override Task Processor_ProcessErrorAsync(ProcessErrorEventArgs arg)
        {
            _logger.LogError(arg.Exception, "ServiceBusQ4Receiver apitomq Error");
            return Task.CompletedTask;
        }

        public override async Task Processor_ProcessMessageAsync(ProcessMessageEventArgs args)
        {
            string body = args.Message.Body.ToString();
            Console.WriteLine($"Q3 Received: {body}");
            var messageModel = JsonConvert.DeserializeObject<ExperimentResult>(body);
            var res = await _experimentsService.UpdateExperimentResultAsync(messageModel);
            if (res)
            {
                await args.CompleteMessageAsync(args.Message);
            }
            else
            {
                await args.DeadLetterMessageAsync(args.Message, "Message is not valid");
                using (_logger.BeginScope(new Dictionary<string, string>()
                    {
                        ["topic"] = TopicPath,
                        ["expt"] = messageModel.ExperimentId,
                        ["reason"] = "result invalid"
                    }
                ))
                {
                    _logger.LogTrace("EXPT RESUL");
                }
            }
        }
    }
}