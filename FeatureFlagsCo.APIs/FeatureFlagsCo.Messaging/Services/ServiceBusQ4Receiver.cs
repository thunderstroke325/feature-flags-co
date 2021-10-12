using Azure.Messaging.ServiceBus;
using FeatureFlagsCo.MQ;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;

namespace FeatureFlagsCo.Messaging.Services
{
    public class ServiceBusQ4Receiver : ServiceBusReceiverBase
    {
        protected override string TopicPath { get { return "q4"; } }
        private ILogger<ServiceBusQ4Receiver> _logger;
        private readonly string _esHost;
        private readonly IFeatureFlagMqService _featureFlagMqService;

        public ServiceBusQ4Receiver(IConfiguration configuration,
                                IFeatureFlagMqService featureFlagMqService,
                                 ILogger<ServiceBusQ4Receiver> logger) : base(configuration)
        {
            _logger = logger;
            _esHost = configuration.GetSection("MySettings").GetSection("ElasticSearchHost").Value;
            _featureFlagMqService = featureFlagMqService;
        }

        public override Task Processor_ProcessErrorAsync(ProcessErrorEventArgs arg)
        {
            _logger.LogError(arg.Exception, "ServiceBusQ4Receiver apitomq Error");
            return Task.CompletedTask;
        }

        public override async Task Processor_ProcessMessageAsync(ProcessMessageEventArgs args)
        {
            string body = args.Message.Body.ToString();
            Console.WriteLine($"Q4 Received: {body}");
            var data = JsonConvert.DeserializeObject<APIServiceToMQServiceModel>(body);
            var completed = true;
            if(data != null && data.Message != null)
            {
                var writeMessageResult = await WriteMessageModelToElasticSearchAsync(data.Message, _esHost);
                if (writeMessageResult == false)
                    return;
            }
            if (data != null && data.FFMessage != null)
            {
                //await _featureFlagMqService.SendMessageAsync(data.FFMessage);
                _featureFlagMqService.SendMessage(data.FFMessage);
            }
            if (completed)
            {
                await args.CompleteMessageAsync(args.Message);
            }
        }

        private async Task<bool> WriteMessageModelToElasticSearchAsync(MessageModel message, string esHost)
        {
            //Console.WriteLine("WriteToElasticSearchAsync");
            var streams = new ExpandoObject();
            if (message.Labels != null && message.Labels.Count > 0)
            {
                foreach (var lable in message.Labels)
                {
                    streams.TryAdd(lable.LabelName, lable.LabelValue);
                }
            }
            dynamic bodyCore = streams;
            Console.WriteLine("Sending message to elastic search");
            using (var client = new HttpClient())
            {
                try
                {
                    client.DefaultRequestHeaders.Accept.TryParseAdd("application/json");
                    HttpContent content = new StringContent(JsonConvert.SerializeObject(bodyCore));
                    content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");
                    if (esHost.Contains("@")) // esHost contains username and password 
                    {
                        var startIndex = esHost.LastIndexOf("/") + 1;
                        var endIndex = esHost.LastIndexOf("@");
                        var credential = esHost.Substring(startIndex, endIndex - startIndex).Split(":");
                        var userName = credential[0];
                        var password = credential[1];

                        esHost = esHost.Substring(0, startIndex) + esHost.Substring(endIndex + 1);

                        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
                                                    "Basic", Convert.ToBase64String(System.Text.ASCIIEncoding.ASCII.GetBytes($"{userName}:{password}")));
                    }
                    //由HttpClient发出异步Post请求
                    //HttpResponseMessage res = await client.PutAsync($"{esHost}/{message.IndexTarget}/_create/{message.FeatureFlagId}", content);
                    HttpResponseMessage res = await client.PostAsync($"{esHost}/{message.IndexTarget}/_doc/", content);
                    Console.WriteLine("Code:" + res.StatusCode.ToString());
                    if (res.StatusCode == System.Net.HttpStatusCode.Created)
                    {
                        return true;
                    }
                }
                catch (Exception exp)
                {
                    Console.WriteLine(exp.Message);
                    await Task.Delay(500);
                }
            }

            return false;
        }

    }
}
