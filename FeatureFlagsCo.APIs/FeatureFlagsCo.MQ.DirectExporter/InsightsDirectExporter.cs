using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Net.Http;
using System.Threading.Tasks;

namespace FeatureFlagsCo.MQ.DirectExporter
{
    public class InsightsDirectExporter : IInsighstMqService
    {
        private readonly string _lokiHostName;
        public InsightsDirectExporter(string lokiHostName = "loki")
        {
            _lokiHostName = lokiHostName;
        }

        public void SendMessage(MessageModel message)
        {
            Console.WriteLine("WriteToGrafanaLokiAsync");
            var streams = new ExpandoObject();
            if (message.Labels != null && message.Labels.Count > 0)
            {
                foreach (var lable in message.Labels)
                {
                    streams.TryAdd(lable.LabelName, lable.LabelValue);
                }
            }
            dynamic bodyCore = new
            {
                stream = streams,
                values = new List<dynamic>()
                            {
                                new List<string>()
                                {
                                    (((DateTimeOffset)message.SendDateTime).ToUnixTimeMilliseconds()).ToString() + "000000",
                                    message.Message
                                }
                            }
            };
            dynamic body = new
            {
                streams = new List<dynamic> {
                        bodyCore
                    }
            };
            Console.WriteLine("Sending message to loki service");
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Accept.TryParseAdd("application/json");
                HttpContent content = new StringContent(JsonConvert.SerializeObject(body));
                content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");
                var run = Task.Run(async () =>
                {
                    int i = 0;
                    while (i < 5)
                    {
                        //由HttpClient发出异步Post请求
                        HttpResponseMessage res = await client.PostAsync($"http://{_lokiHostName}:3100/loki/api/v1/push", content);
                        if (res.StatusCode == System.Net.HttpStatusCode.NoContent)
                        {
                            Console.WriteLine("Message Sent.");
                            break;
                        }
                        await Task.Delay(500);
                        i++;
                    }
                });
                run.Wait();
            }
        }

    }


}
