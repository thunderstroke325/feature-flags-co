using FeatureFlags.APIs.ViewModels;
using FeatureFlagsCo.RabbitMqModels;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FeatureFlags.APIs.Services
{
    public interface IInsighstRabbitMqService
    {
        void SendMessage(MessageModel message);
    }

    public class InsighstRabbitMqService : IInsighstRabbitMqService
    {
        private readonly ConnectionFactory _connectionFactory;
        private readonly IOptions<MySettings> _mySettings;
        private IConnection _connection;
        private IModel _channel;
        public InsighstRabbitMqService(IOptions<MySettings> mySettings)
        {
            _mySettings = mySettings;
            _connectionFactory = new ConnectionFactory() { HostName = _mySettings.Value.InsightsRabbitMqUrl };
            _connection = _connectionFactory.CreateConnection();
            _connection.CallbackException += Connection_CallbackException;
            _connection.ConnectionShutdown += Connection_ConnectionShutdown;
            _connection.ConnectionBlocked += Connection_ConnectionBlocked;
            _channel = _connection.CreateModel();
            _channel.QueueDeclare(queue: "hello",
                                    durable: false,
                                    exclusive: false,
                                    autoDelete: false,
                                    arguments: null);
            _channel.CallbackException += Channel_CallbackException;
        }

        private void Channel_CallbackException(object sender, RabbitMQ.Client.Events.CallbackExceptionEventArgs e)
        {
            _channel = _connection.CreateModel();
            _channel.QueueDeclare(queue: "hello",
                                    durable: false,
                                    exclusive: false,
                                    autoDelete: false,
                                    arguments: null);
        }

        private void Connection_ConnectionBlocked(object sender, RabbitMQ.Client.Events.ConnectionBlockedEventArgs e)
        {
            _connection.Abort();
            _connection.Close();
            _connection = _connectionFactory.CreateConnection();
        }

        private void Connection_ConnectionShutdown(object sender, ShutdownEventArgs e)
        {
            _connection = _connectionFactory.CreateConnection();
        }

        private void Connection_CallbackException(object sender, RabbitMQ.Client.Events.CallbackExceptionEventArgs e)
        {
            _connection = _connectionFactory.CreateConnection();
        }

        public void SendMessage(MessageModel message)
        {

            var body = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(message));

            _channel.BasicPublish(exchange: "",
                                  routingKey: "hello",
                                  basicProperties: null,
                                  body: body);

            Console.WriteLine(message);
        }

    }
}
