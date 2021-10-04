
using FeatureFlagsCo.MQ;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FeatureFlagsCo.MQ.Export
{
    public interface IExportInsightsDataToElasticSearchService
    {
        void Init();
    }
    public class ExportInsightsDataToElasticSearchService: IExportInsightsDataToElasticSearchService
    {
        private readonly ConnectionFactory _factory;
        private IConnection _connection;
        private IModel _channel;
        private readonly string _esHost;
        public ExportInsightsDataToElasticSearchService(string rabbitConnectStr = "amqp://localhost:5672/", string esHost = "http://localhost:9200")
        {
            _factory = new ConnectionFactory();
            _factory.Uri = new Uri(rabbitConnectStr);
            _esHost = esHost;

            Init();
        }
        public void Init()
        {
            if (_channel != null)
            {
                _channel.Close();
                _channel.QueueDelete("hello");
            }
            if (_connection != null)
                _connection.Close();
            for (int i = 0; i < 3; i++)
            {
                Console.WriteLine("Start RabbitMq Receiver Service at " + DateTime.UtcNow.ToString());
                try
                {
                    _connection = _factory.CreateConnection();
                    _channel = _connection.CreateModel();

                    _connection.ConnectionShutdown += (sender, e) =>
                    {
                        Console.WriteLine("ConnectionShutdown: " + e.ReplyText);
                        Init();
                    };
                    _channel.ModelShutdown += (sender, e) =>
                    {
                        Console.WriteLine("ModelShutdown: " + e.ReplyText);
                        Init();
                    };


                    Console.WriteLine("Connection and channel created");

                    // Q4 同步feature flag 数据
                    _channel.ExchangeDeclare(exchange: "Q4", type: "topic");
                    var queueName = _channel.QueueDeclare(queue: "hello",
                        durable: false,
                        exclusive: false,
                        autoDelete: false,
                        arguments: null).QueueName;

                    _channel.QueueBind(queue: queueName,
                        exchange: "Q4",
                        routingKey: "es.experiments.events.ff.#");

                    _channel.QueueDeclare(queue: "hello",
                                         durable: false,
                                         exclusive: false,
                                         autoDelete: false,
                                         arguments: null);

                    var consumer = new EventingBasicConsumer(_channel);
                    consumer.Received += async (model, ea) =>
                    {
                        Console.WriteLine("New message received");
                        string message = "";
                        try
                        {
                            var body = ea.Body.ToArray();
                            message = Encoding.UTF8.GetString(body);
                            Console.WriteLine(message);
                            var messageModel = JsonConvert.DeserializeObject<MessageModel>(message);
                            await WriteToElasticSearchAsync(messageModel, _esHost);
                            _channel.BasicAck(ea.DeliveryTag, false);
                        }
                        catch (AggregateException aexp)
                        {
                            Console.WriteLine("New message exception:");
                            Console.WriteLine(aexp.Message);
                            Console.WriteLine(message);
                        }
                        catch (Exception exp)
                        {
                            Console.WriteLine("New message exception:");
                            Console.WriteLine(exp.Message);
                            Console.WriteLine(message);
                        }

                    };
                    _channel.BasicConsume(queue: "hello",
                                         autoAck: false,
                                         consumer: consumer);

                    break;
                }
                catch (Exception exp)
                {
                    Console.WriteLine($"{i} times. Connection failed:" + exp.Message);
                    Thread.Sleep(30 * 1000);
                }
            }
        }

        private async Task WriteToElasticSearchAsync(MessageModel message, string esHost)
        {
            Console.WriteLine("WriteToElasticSearchAsync");
            var streams = new ExpandoObject();
            if (message.Labels != null && message.Labels.Count > 0)
            {
                foreach (var lable in message.Labels)
                {
                    streams.TryAdd(lable.LabelName, lable.LabelValue);
                }
            }
            dynamic bodyCore = streams;
            //dynamic body = new
            //{
            //    streams = new List<dynamic> {
            //            bodyCore
            //        }
            //};
            int i = 0;
            while (i < 5)
            {
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
                            Console.WriteLine("Message Sent.");
                            break;
                        }
                        await Task.Delay(500);
                    }
                    catch(Exception exp)
                    {
                        Console.WriteLine(exp.Message);
                    }
                }
                i++;
            }
        }
    }
}
