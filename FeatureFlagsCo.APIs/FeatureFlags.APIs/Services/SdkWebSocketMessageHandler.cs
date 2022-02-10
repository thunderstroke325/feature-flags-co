using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using FeatureFlags.APIs.Models;
using FeatureFlags.APIs.Repositories;
using FeatureFlags.APIs.Services.MongoDb;
using FeatureFlags.Utils.ConventionalDependencyInjection;
using FeatureFlags.Utils.ExtensionMethods;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace FeatureFlags.APIs.Services
{
    public class SdkWebSocketMessageHandler : ITransientDependency
    {
        private readonly IEnumerable<IMessageHandler> _handlers;
        private readonly ILogger<SdkWebSocketMessageHandler> _logger;

        public SdkWebSocketMessageHandler(
            IEnumerable<IMessageHandler> handlers,
            ILogger<SdkWebSocketMessageHandler> logger)
        {
            _handlers = handlers;
            _logger = logger;
        }

        public async Task HandleAsync(SdkWebSocketMessageContext context)
        {
            var message = context.Message;
            var messageType = message["messageType"]?.ToString();

            var handler = _handlers.FirstOrDefault(handler => handler.MessageType == messageType);
            if (handler == null)
            {
                var exp = new SdkWebSocketMessageHandleException($"no message handler for type '{messageType}'");
                _logger.LogError(exp, exp.Message);
                return;
            }

            try
            {
                await handler.HandleAsync(context);

                _logger.LogDebug(
                    "message {0} from {1} has been handled successfully",
                    message.ToString(Formatting.None), context.Socket.ConnectionId
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "exception errored when handle sdk websocket message {0}", message.ToString());
            }
        }
    }

    public class SdkWebSocketMessageContext
    {
        public IServiceProvider ServiceProvider { get; set; }

        public SdkWebSocket Socket { get; set; }

        public JObject Message { get; set; }

        public SdkWebSocketMessageContext(
            IServiceProvider serviceProvider,
            SdkWebSocket socket,
            JObject message)
        {
            ServiceProvider = serviceProvider;
            Message = message;
            Socket = socket;
        }
    }

    public interface IMessageHandler : ITransientDependency
    {
        string MessageType { get; }

        Task HandleAsync(SdkWebSocketMessageContext context);
    }

    public class DataSyncMessageHandler : IMessageHandler
    {
        public string MessageType => SdkWebSocketMessageTypes.DataSync;

        public async Task HandleAsync(SdkWebSocketMessageContext context)
        {
            var message = context.Message;
            var socket = context.Socket;

            var request = message["data"]?.ToObject<SdkDataSync>();
            if (request == null)
            {
                throw new ArgumentException($"invalid client message {message}");
            }

            var eventType = request.Timestamp == 0 ? "full" : "patch";
            object data = socket.SdkType switch
            {
                SdkTypes.Client => new
                {
                    eventType, 
                    featureFlags = await GetClientSdkFlagsAsync(socket, request, context.ServiceProvider)
                },
                SdkTypes.Server => new
                {
                    eventType, 
                    featureFlags = await GetServerSdkFlagsAsync(socket, request, context.ServiceProvider)
                }
            };

            var response = new SdkWebSocketMessage(SdkWebSocketMessageTypes.DataSync, data);
            await socket.SendAsync(response);
        }

        private async Task<IEnumerable<ClientSdkFeatureFlag>> GetClientSdkFlagsAsync(
            SdkWebSocket socket,
            SdkDataSync request,
            IServiceProvider serviceProvider)
        {
            if (request.User == null)
            {
                throw new ArgumentException($"client sdk must attach user info when sync data, socket {socket}");
            }

            var envSecret = socket.EnvSecret;
            
            var mongoDb = serviceProvider.GetRequiredService<MongoDbPersist>();
            var flags = await GetActiveFlagsAsync(mongoDb, envSecret, request.Timestamp);
            
            var variationService = serviceProvider.GetRequiredService<IVariationService>();

            var result = new List<ClientSdkFeatureFlag>(); 
            foreach (var flag in flags)
            {
                var ffIdVm =
                    FeatureFlagKeyExtension.GetFeatureFlagIdByEnvironmentKey(envSecret, flag.FF.KeyName);

                var userVariation = 
                    await variationService.GetUserVariationAsync(envSecret, request.User.EnvironmentUser(), ffIdVm);

                var clientSdkFeatureFlag = new ClientSdkFeatureFlag
                {
                    Id = flag.FF.KeyName,
                    Variation = userVariation.Variation.VariationValue, 
                    SendToExperiment = userVariation.SendToExperiment,
                    IsArchived = flag.IsArchived,
                    Options = flag.VariationOptions.Select(option => new ClientSdkVariation
                    {
                        Id = option.LocalId,
                        Value = option.VariationValue
                    }),
                    Timestamp = flag.FF.LastUpdatedTime?.UnixTimestampInMilliseconds() ?? 0
                };
                
                result.Add(clientSdkFeatureFlag);
            }

            return result.OrderByDescending(flag => flag.Timestamp);
        }

        private async Task<IEnumerable<ServerSdkFeatureFlag>> GetServerSdkFlagsAsync(
            SdkWebSocket socket,
            SdkDataSync request,
            IServiceProvider serviceProvider)
        {
            var mongoDb = serviceProvider.GetRequiredService<MongoDbPersist>();
            var flags = await GetActiveFlagsAsync(mongoDb, socket.EnvSecret, request.Timestamp);

            var mapper = serviceProvider.GetRequiredService<IMapper>();
            
            var serverSdkFlags = mapper.Map<IEnumerable<FeatureFlag>, IEnumerable<ServerSdkFeatureFlag>>(flags);
            return serverSdkFlags.OrderByDescending(flag => flag.Timestamp);
        }

        private async Task<IEnumerable<FeatureFlag>> GetActiveFlagsAsync(
            MongoDbPersist mongoDb,
            string envSecret, 
            long timestamp)
        {
            var envId = EnvironmentSecretV2.Parse(envSecret).EnvId;

            var lastModified = DateTime.UnixEpoch.AddMilliseconds(timestamp);

            var flags = await mongoDb.QueryableOf<FeatureFlag>()
                .Where(flag => flag.EnvironmentId == envId && !flag.IsArchived)
                .Where(flag => flag.FF.LastUpdatedTime > lastModified)
                .ToListAsync();

            return flags;
        }
    }
}