using FeatureFlagsCo.Messaging;
using FeatureFlagsCo.MQ;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using RabbitMQ.Client;
using System;
using System.Text;

namespace FeatureFlags.APIs.Services
{

    public class AuditLogMqService : IAuditLogMqService
    {
        private readonly ConnectionFactory _connectionFactory;
        private readonly IOptions<MySettings> _mySettings;
        private IConnection _connection;
        private IModel _channel;
        private string _queueName;
        private readonly ILogger<AuditLogMqService> _logger;
        public AuditLogMqService(IOptions<MySettings> mySettings,
            ILogger<AuditLogMqService> logger)
        {
            _mySettings = mySettings;
            _logger = logger;

            _connectionFactory = new ConnectionFactory();
            _connectionFactory.Uri = new Uri(_mySettings.Value.InsightsRabbitMqUrl);
            _connection = _connectionFactory.CreateConnection();
            _connection.CallbackException += Connection_CallbackException;
            _connection.ConnectionShutdown += Connection_ConnectionShutdown;
            _connection.ConnectionBlocked += Connection_ConnectionBlocked;
            _channel = _connection.CreateModel();

            _queueName = "auditlog" + (Environment.MachineName ?? "");
            _channel.QueueDeclare(queue: _queueName,
                                    durable: false,
                                    exclusive: false,
                                    autoDelete: false,
                                    arguments: null);
            _channel.CallbackException += Channel_CallbackException;
        }

        private void Channel_CallbackException(object sender, RabbitMQ.Client.Events.CallbackExceptionEventArgs e)
        {
            _channel = _connection.CreateModel();
            _channel.QueueDeclare(queue: _queueName,
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

        public void Log(AuditLogMessageModel message)
        {
            try
            {
                message.TimeStamp = (Int64)(DateTime.UtcNow.AddDays(-1).Subtract(new DateTime(1970, 1, 1))).TotalMilliseconds;
                var body = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(message));

                _channel.BasicPublish(exchange: "",
                                      routingKey: _queueName,
                                      basicProperties: null,
                                      body: body);
                //_channel.BasicPublish(exchange: "",
                //                      routingKey: "experiments",
                //                      basicProperties: null,
                //                      body: body);

                //Console.WriteLine(message);
            }
            catch (Exception exp)
            {
                _logger.LogError(exp, exp.Message);
            }
        }
    }
}
