using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Threading.Tasks;

namespace FeatureFlagsCo.Messaging.Services
{
    public abstract class ServiceBusReceiverBase
    {
        protected abstract string TopicPath { get; }

        private readonly ServiceBusClient _client;
        private ServiceBusProcessor _processor;

        public ServiceBusReceiverBase(
            IConfiguration configuration
            )
        {
            _client = new ServiceBusClient(configuration.GetSection("MySettings").GetSection("ServiceBusConnectionString").Value);

            Task.Run(async () =>
            {
                await StartProcessAsync(_client);
            }).ConfigureAwait(true);
        }

        public async Task StartProcessAsync(ServiceBusClient client)
        {
            _processor = client.CreateProcessor(TopicPath, "standard", new ServiceBusProcessorOptions());
            try
            {
                // add handler to process messages
                _processor.ProcessMessageAsync += Processor_ProcessMessageAsync;
                // add handler to process any errors
                _processor.ProcessErrorAsync += Processor_ProcessErrorAsync; ;

                await _processor.StartProcessingAsync();
            }
            finally
            {
            }
            // start processing 
        }

        public abstract Task Processor_ProcessErrorAsync(ProcessErrorEventArgs arg);

        public abstract Task Processor_ProcessMessageAsync(ProcessMessageEventArgs args);
    }
}
