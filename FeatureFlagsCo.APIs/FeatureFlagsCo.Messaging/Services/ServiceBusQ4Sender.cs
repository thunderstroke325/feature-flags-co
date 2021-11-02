using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Net.Http;
using System.Net.Http.Headers;
using FeatureFlagsCo.MQ;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using Newtonsoft.Json;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace FeatureFlagsCo.Messaging.Services
{
    public class ServiceBusQ4Sender : ServiceBusTopicSenderBase
    {
        private readonly string _esHost;

        protected override string TopicPath
        {
            get { return Configuration.GetSection("MySettings").GetSection("Q4Name").Value; }
        }

        public ServiceBusQ4Sender(IConfiguration configuration, ILogger<ServiceBusQ4Sender> logger) : base(
            configuration, logger)
        {
            _esHost = configuration.GetSection("MySettings").GetSection("ElasticSearchHost").Value;
        }

        //public async Task SendMessageAsync(MessageModel model)
        //{
        //    string messagePayload = JsonSerializer.Serialize(model);
        //    ServiceBusMessage message = new ServiceBusMessage(messagePayload);
        //    await _clientSender.SendMessageAsync(message).ConfigureAwait(false);
        //}

        public async Task SendAPIServiceToMQServiceData(APIServiceToMQServiceModel data)
        {
            if (data != null && data.Message != null)
            {
                var writeMessageResult = await WriteMessageModelToElasticSearchAsync(data.Message, _esHost);
                if (writeMessageResult == false)
                    return;
            }

            if (data != null && data.FFMessage != null)
            {
                string messagePayload = JsonSerializer.Serialize(data.FFMessage);
                await SendMessageAsync(messagePayload);
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
                            "Basic",
                            Convert.ToBase64String(System.Text.ASCIIEncoding.ASCII.GetBytes($"{userName}:{password}")));
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