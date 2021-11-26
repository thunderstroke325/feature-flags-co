using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;
using FeatureFlagsCo.Messaging.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using StackExchange.Redis;

namespace FeatureFlagsCo.Messaging.Services
{
    public class ServiceBusQ3Receiver : ServiceBusReceiverBase
    {
        protected override string TopicPath
        {
            get { return Configuration.GetSection("MySettings").GetSection("Q3Name").Value; }
        }

        private readonly ExperimentsService _experimentsService;

        public ServiceBusQ3Receiver(IConfiguration configuration,
            ExperimentsService experimentsService,
            ILogger<ServiceBusQ3Receiver> logger,
            IConnectionMultiplexer redis) : base(configuration, logger, redis)
        {
            _experimentsService = experimentsService;
            Logger.LogInformation("RECEIVER START: {topic} {subscription}", TopicPath, "standard");
        }

        public override async Task Processor_ProcessErrorAsync(ProcessErrorEventArgs arg)
        {
            Logger.LogError(arg.Exception, "Q3 ERROR");
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
                Logger.LogInformation("RESULT OK: {topic} {expt}", TopicPath, messageModel.ExperimentId);
            }
            else
            {
                await args.DeadLetterMessageAsync(args.Message, "RESULT NOT UPDATE");
                Logger.LogError("RESULT NOT UPDATE: {topic} {expt}", TopicPath, messageModel.ExperimentId);
            }
        }

        public override async Task HandleMessageAsync(string result)
        {
            Console.WriteLine($"Q3 Received: {result}");
            var messageModel = JsonConvert.DeserializeObject<ExperimentResult>(result);
            var res = await _experimentsService.UpdateExperimentResultAsync(messageModel);
            if (res)
            {
                Logger.LogInformation("RESULT OK: {topic} {expt}", TopicPath, messageModel.ExperimentId);
            }
            else
            {
                var msg = Encoding.UTF8.GetBytes(result);
                var db = Redis.GetDatabase();
                await db.ListRightPushAsync($"dead_letters_{TopicPath}", msg);
                Logger.LogError("RESULT NOT UPDATE: {topic} {expt}", TopicPath, messageModel.ExperimentId);
            }
        }

        public override async Task HandleErrorAsync(string result, Exception e)
        {
            try
            {
                if (!String.IsNullOrEmpty(result))
                {
                    var msg = Encoding.UTF8.GetBytes(result);
                    var db = Redis.GetDatabase();
                    var latency = await db.PingAsync();
                    if (latency.Milliseconds < Redis.TimeoutMilliseconds)
                    {
                        await db.ListRightPushAsync($"dead_letters_{TopicPath}", msg);
                    }
                }
                Logger.LogError(e, "Q3 ERROR");
            }
            catch (Exception exp)
            {
                Logger.LogError(exp.Message);
            }
        }
    }
}