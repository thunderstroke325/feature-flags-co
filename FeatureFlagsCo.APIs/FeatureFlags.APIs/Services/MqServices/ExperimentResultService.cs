using System;
using System.Text;
using System.Threading;
using FeatureFlags.APIs.Models;
using FeatureFlags.APIs.ViewModels;
using FeatureFlags.APIs.ViewModels.Experiments;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace FeatureFlags.APIs.Services
{
    public interface IExperimentResultService
    {
        void Init();
    }

    public class ExperimentResultService : IExperimentResultService
    {
        private readonly ConnectionFactory _factory;
        private readonly IOptions<MySettings> _mySettings;
        private readonly IExperimentsService _experimentService;
        private IConnection _connection;
        private IModel _channel;

        public ExperimentResultService(IOptions<MySettings> mySettings, IExperimentsService experimentService)
        {
            _mySettings = mySettings;
            _factory = new ConnectionFactory();
            _factory.Uri = new Uri(_mySettings.Value.InsightsRabbitMqUrl);

            _experimentService = experimentService;
        }

        public void Init()
        {
            if (_channel != null)
            {
                _channel.Close();
                _channel.QueueDelete("experiment.result.reader");
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

                    // Q3 get expt result
                    _channel.ExchangeDeclare(exchange: "Q3", type: "topic");
                    var queueName = _channel.QueueDeclare(queue: "experiment.result.reader",
                        durable: false,
                        exclusive: false,
                        autoDelete: false,
                        arguments: null).QueueName;
                    _channel.QueueBind(queue: queueName,
                        exchange: "Q3",
                        routingKey: "py.experiments.experiment.results.#");
                    var consumer = new EventingBasicConsumer(_channel);
                    consumer.Received += async (model, ea) =>
                    {
                        string message = default;
                        try
                        {
                            var body = ea.Body.ToArray();
                            message = Encoding.UTF8.GetString(body);
                            var messageModel = JsonConvert.DeserializeObject<ExperimentResult>(message);
                            await _experimentService.UpdateExperimentResultAsync(messageModel);

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
                    _channel.BasicConsume(queue: "queueName",
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
    }
}