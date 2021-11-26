using System;
using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using StackExchange.Redis;

namespace FeatureFlagsCo.Messaging.Services
{
    public abstract class ServiceBusReceiverBase
    {
        protected abstract string TopicPath { get; }

        protected ILogger Logger { get; }

        private readonly ServiceBusClient _client;
        private ServiceBusProcessor _processor;
        private readonly string _engine;
        
        protected IConnectionMultiplexer Redis { get; }
        protected IConfiguration Configuration { get; }

        public ServiceBusReceiverBase(IConfiguration configuration, ILogger logger, IConnectionMultiplexer redis)
        {
            Configuration = configuration;
            Logger = logger;
            _engine = configuration.GetSection("MySettings:BusType").Value;

            if ("azure".Equals(_engine))
            {
                _client = 
                    new ServiceBusClient(Configuration.GetSection("MySettings").GetSection("ServiceBusConnectionString").Value);
            }
            else
            {
                Redis = redis;
            }

            Task.Run(async () =>
            {
                if ("azure".Equals(_engine))
                {
                    await AzureServiceBusStartProcessAsync(_client);
                }
                else
                {
                    await RedisStartProcessAsync();
                }
            }).ConfigureAwait(true);
        }

        private async Task RedisStartProcessAsync()
        {
            while (true)
            {
                string message = null;
                try
                {
                    var db = Redis.GetDatabase();
                    var bytes = await db.ListLeftPopAsync(TopicPath);
                    if (!bytes.IsNull)
                    {
                        message = System.Text.Encoding.UTF8.GetString(bytes);
                        await HandleMessageAsync(message);
                    }
                    else
                    {
                        // No message delay 1s
                        await Task.Delay(1000);
                    }
                }
                catch (Exception e)
                {
                    await HandleErrorAsync(message, e);
                }
            }
        }


        private async Task AzureServiceBusStartProcessAsync(ServiceBusClient client)
        {
            _processor = client.CreateProcessor(TopicPath, "standard", new ServiceBusProcessorOptions()
            {
                PrefetchCount = 5,
                AutoCompleteMessages = false
            });
            try
            {
                // add handler to process messages
                _processor.ProcessMessageAsync += Processor_ProcessMessageAsync;
                // add handler to process any errors
                _processor.ProcessErrorAsync += Processor_ProcessErrorAsync;
                ;

                await _processor.StartProcessingAsync();
            }
            catch (Exception ex)
            {
                Logger.LogError("{topic} RECEIVER FAIL: {error}", TopicPath, ex.Message);
            }
            // start processing 
        }

        public abstract Task HandleMessageAsync(string result);

        public abstract Task HandleErrorAsync(string result, Exception e);

        public abstract Task Processor_ProcessErrorAsync(ProcessErrorEventArgs arg);

        public abstract Task Processor_ProcessMessageAsync(ProcessMessageEventArgs args);
    }
}