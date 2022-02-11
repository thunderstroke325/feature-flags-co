using System;
using System.Collections.Generic;
using System.Linq;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using System.Threading.Tasks;
using AutoMapper;
using FeatureFlags.APIs.Models;
using FeatureFlags.APIs.Repositories;
using FeatureFlags.APIs.Services.MongoDb;
using FeatureFlags.Utils.ConventionalDependencyInjection;

namespace FeatureFlags.APIs.Services
{
    public class SdkSyncDataService : ITransientDependency
    {
        private readonly MongoDbPersist _mongoDb;
        private readonly IVariationService _variationService;
        private readonly IMapper _mapper;

        public SdkSyncDataService(
            MongoDbPersist mongoDb,
            IVariationService variationService, 
            IMapper mapper)
        {
            _variationService = variationService;
            _mapper = mapper;
            _mongoDb = mongoDb;
        }

        public async Task<object> GetSdkDataAsync(
            string eventType,
            FeatureFlag flag, 
            SdkWebSocket socket)
        {
            if (socket.SdkType == SdkTypes.Client && socket.User == null)
            {
                throw new ArgumentException($"client sdk must have user info when sync data, socket {socket}");
            }

            object data = socket.SdkType switch
            {
                SdkTypes.Client => new
                {
                    eventType,
                    featureFlags = new[] { await GetClientSdkDataAsync(flag, socket) }
                },
                SdkTypes.Server => new
                {
                    eventType,
                    featureFlags = new[] { GetServerSdkData(flag) }
                }
            };

            return data;
        }

        private async Task<ClientSdkFeatureFlag> GetClientSdkDataAsync(FeatureFlag flag, SdkWebSocket socket)
        {
            var ffIdVm =
                FeatureFlagKeyExtension.GetFeatureFlagIdByEnvironmentKey(socket.EnvSecret, flag.FF.KeyName);
            
            var userVariation =
                await _variationService.GetUserVariationAsync(socket.User?.EnvironmentUser(), ffIdVm);

            var clientSdkFeatureFlag = new ClientSdkFeatureFlag(flag, userVariation);

            return clientSdkFeatureFlag;
        }

        private ServerSdkFeatureFlag GetServerSdkData(FeatureFlag flag)
        {
            var sdkFlag = _mapper.Map<FeatureFlag, ServerSdkFeatureFlag>(flag);

            return sdkFlag;
        }

        public async Task<object> GetSdkDataAsync(SdkWebSocket socket, SdkDataSyncRequest request)
        {
            // timestamp cannot be negative
            if (request.Timestamp < 0)
            {
                throw new ArgumentException(
                    $"receive invalid timestamp {request.Timestamp} when sync data, socket {socket}");
            }

            // client sdk should attach user info
            if (socket.SdkType == SdkTypes.Client && request.User == null)
            {
                throw new ArgumentException($"client sdk must attach user info when sync data, socket {socket}");
            }

            socket.AttachUser(request.User);

            var eventType = request.Timestamp == 0 ? "full" : "patch";
            object data = socket.SdkType switch
            {
                SdkTypes.Client => new
                {
                    eventType,
                    featureFlags = await GetClientSdkDataAsync(socket, request.Timestamp)
                },
                SdkTypes.Server => new
                {
                    eventType,
                    featureFlags = await GetServerSdkDataAsync(socket.EnvId, request.Timestamp)
                }
            };

            return data;
        }

        private async Task<IEnumerable<ServerSdkFeatureFlag>> GetServerSdkDataAsync(int envId, long timestamp)
        {
            var flags = await GetActiveFlagsAsync(envId, timestamp);

            var sdkFlags = _mapper.Map<IEnumerable<FeatureFlag>, IEnumerable<ServerSdkFeatureFlag>>(flags);

            return sdkFlags.OrderByDescending(flag => flag.Timestamp);
        }

        private async Task<IEnumerable<ClientSdkFeatureFlag>> GetClientSdkDataAsync(SdkWebSocket socket, long timestamp)
        {
            var flags = await GetActiveFlagsAsync(socket.EnvId, timestamp);
            var result = new List<ClientSdkFeatureFlag>();
            foreach (var flag in flags)
            {
                var ffIdVm =
                    FeatureFlagKeyExtension.GetFeatureFlagIdByEnvironmentKey(socket.EnvSecret, flag.FF.KeyName);

                var userVariation =
                    await _variationService.GetUserVariationAsync(socket.User?.EnvironmentUser(), ffIdVm);

                var clientSdkFeatureFlag = new ClientSdkFeatureFlag(flag, userVariation);

                result.Add(clientSdkFeatureFlag);
            }

            return result.OrderByDescending(flag => flag.Timestamp);
        }

        private async Task<IEnumerable<FeatureFlag>> GetActiveFlagsAsync(int envId, long timestamp)
        {
            var lastModified = DateTime.UnixEpoch.AddMilliseconds(timestamp);

            var flags = await _mongoDb.QueryableOf<FeatureFlag>()
                .Where(flag => flag.EnvironmentId == envId && !flag.IsArchived)
                .Where(flag => flag.FF.LastUpdatedTime > lastModified)
                .ToListAsync();

            return flags;
        }
    }
}