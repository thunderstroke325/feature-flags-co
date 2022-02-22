using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using FeatureFlags.APIs.Models;
using FeatureFlags.Utils.ConventionalDependencyInjection;

namespace FeatureFlags.APIs.Services
{
    public class SdkWebSocketConnectionManager : ISingletonDependency
    {
        // envId -> SdkWebSocket[], SdkWebSocket GroupBy envId
        private readonly ConcurrentDictionary<int, ImmutableList<SdkWebSocket>> _sockets;

        public SdkWebSocketConnectionManager()
        {
            _sockets = new ConcurrentDictionary<int, ImmutableList<SdkWebSocket>>();
        }

        public IEnumerable<SdkWebSocket> GetAll() => _sockets.Values.SelectMany(x => x);

        public void RegisterSocket(SdkWebSocket socket)
        {
            var envId = socket.EnvId;

            _sockets.AddOrUpdate(envId, ImmutableList.Create(socket), (_, existing) => existing.Add(socket));
        }

        public IEnumerable<SdkWebSocket> GetSockets(int envId)
        {
            if (_sockets.TryGetValue(envId, out var sockets))
            {
                return sockets;
            }
            
            return Array.Empty<SdkWebSocket>();
        }

        public async Task UnregisterSocket(SdkWebSocket socket)
        {
            var envId = socket.EnvId;
            if (!_sockets.TryGetValue(envId, out var envSockets))
            {
                return;
            }
            
            var socketToRemove = envSockets.FirstOrDefault(x => x.ConnectionId == socket.ConnectionId);
            if (socketToRemove != null)
            {
                await socketToRemove.CloseSafelyAsync();
            }
            
            _sockets.AddOrUpdate(
                envId,
                ImmutableList<SdkWebSocket>.Empty,
                (_, existing) => existing.RemoveAll(x => x.ConnectionId == socket.ConnectionId)
            );
        }
    }
}