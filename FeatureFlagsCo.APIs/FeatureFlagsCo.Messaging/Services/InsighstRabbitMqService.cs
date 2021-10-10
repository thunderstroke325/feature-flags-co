using FeatureFlagsCo.MQ;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using RabbitMQ.Client;
using System;
using System.Text;
using System.Threading.Tasks;

namespace FeatureFlagsCo.Messaging.Services
{

    public class InsighstRabbitMqService : IInsighstMqService
    {
        private readonly ConnectionFactory _connectionFactory;
        private readonly IOptions<MySettings> _mySettings;
        private IConnection _connection;
        private IModel _channel;
        public InsighstRabbitMqService(IOptions<MySettings> mySettings)
        {
            _mySettings = mySettings;

            _connectionFactory = new ConnectionFactory();
            _connectionFactory.Uri = new Uri(_mySettings.Value.InsightsRabbitMqUrl);
            _connection = _connectionFactory.CreateConnection();
            _connection.CallbackException += Connection_CallbackException;
            _connection.ConnectionShutdown += Connection_ConnectionShutdown;
            _connection.ConnectionBlocked += Connection_ConnectionBlocked;
            _channel = _connection.CreateModel();
            // _channel.QueueDeclare(queue: "hello",
            //                         durable: false,
            //                         exclusive: false,
            //                         autoDelete: false,
            //                         arguments: null);
            //this.SendMessage(new MessageModel
            //{
            //    Labels = new List<MessageLabel> { new MessageLabel { LabelName = "API Init", LabelValue = DateTime.UtcNow.ToLongDateString() } },
            //    Message = "InsighstRabbitMqService Connected",
            //    SendDateTime = DateTime.UtcNow
            //});
            _channel.CallbackException += Channel_CallbackException;
        }

        private void Channel_CallbackException(object sender, RabbitMQ.Client.Events.CallbackExceptionEventArgs e)
        {
            _channel = _connection.CreateModel();
            // _channel.QueueDeclare(queue: "hello",
            //                         durable: false,
            //                         exclusive: false,
            //                         autoDelete: false,
            //                         arguments: null);
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
            // Q4 数据发送至es
            _channel.ExchangeDeclare(exchange: "Q4", type: "topic");
            _channel.BasicPublish(exchange: "Q4",
                routingKey: "es.experiments.events.ff",
                basicProperties: null,
                body: body);
            // _channel.BasicPublish(exchange: "",
            //                       routingKey: "hello",
            //                       basicProperties: null,
            //                       body: body);
            //_channel.BasicPublish(exchange: "",
            //                      routingKey: "hello",
            //                      basicProperties: null,
            //                      body: body);

            //Console.WriteLine(message);
        }

        public Task SendMessageAsync(MessageModel message)
        {
            Task.Yield();
            SendMessage(message);

            return Task.FromResult<Object>(null);
        }
    }
}
