using FeatureFlagsCo.RabbitMqModels;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FeatureFlagsCo.RabbitMQToGrafanaLoki
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                var factory = new ConnectionFactory() { HostName = "localhost" };
                using (var connection = factory.CreateConnection())
                using (var channel = connection.CreateModel())
                {
                    channel.QueueDeclare(queue: "hello",
                                         durable: false,
                                         exclusive: false,
                                         autoDelete: false,
                                         arguments: null);

                    var consumer = new EventingBasicConsumer(channel);
                    consumer.Received += async (model, ea) =>
                    {
                        var body = ea.Body.ToArray();
                        var message = Encoding.UTF8.GetString(body);
                        await WriteToGrafanaLokiAsync(JsonConvert.DeserializeObject<MessageModel>(message));

                    };
                    channel.BasicConsume(queue: "hello",
                                         autoAck: true,
                                         consumer: consumer);

                    Console.WriteLine(" Press [enter] to exit.");
                    Console.ReadLine();
                }
            }
            catch(Exception exp)
            {
                Console.WriteLine("Exception:");
                Console.WriteLine(exp.Message, exp);
            }

        }

        static async Task WriteToGrafanaLokiAsync(MessageModel message)
        {
            try
            {
                Console.WriteLine(message);
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
                int i = 0;
                while (i < 5)
                {
                    using (var client = new HttpClient())
                    {
                        client.DefaultRequestHeaders.Accept.TryParseAdd("application/json");
                        HttpContent content = new StringContent(JsonConvert.SerializeObject(body));
                        content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");
                        //由HttpClient发出异步Post请求
                        HttpResponseMessage res = await client.PostAsync("http://localhost:3100/loki/api/v1/push", content);
                        if (res.StatusCode == System.Net.HttpStatusCode.NoContent)
                        {
                            break;
                        }
                        await Task.Delay(500);
                    }
                    i++;
                }
            }
            catch(Exception exp)
            {
                Console.WriteLine("Exception:");
                Console.WriteLine(exp.Message, exp);
            }
        }

    }
}
