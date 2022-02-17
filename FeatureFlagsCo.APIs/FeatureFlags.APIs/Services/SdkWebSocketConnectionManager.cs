using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FeatureFlags.APIs.Models;
using FeatureFlags.Utils.ConventionalDependencyInjection;

namespace FeatureFlags.APIs.Services
{
    public class SdkWebSocketConnectionManager : ISingletonDependency
    {
        // envId -> SdkWebSocket[], SdkWebSocket GroupBy envId
        private readonly ConcurrentDictionary<int, List<SdkWebSocket>> _sockets;

        public SdkWebSocketConnectionManager()
        {
            _sockets = new ConcurrentDictionary<int, List<SdkWebSocket>>();
        }

        public void RegisterSocket(SdkWebSocket socket)
        {
            var envId = socket.EnvId;

            if (_sockets.ContainsKey(envId))
            {
                _sockets[envId].Add(socket);
            }
            else
            {
                _sockets[envId] = new List<SdkWebSocket> { socket };
            }
        }

        public IEnumerable<SdkWebSocket> GetSockets(int envId)
        {
            if (!_sockets.ContainsKey(envId))
            {
                return Array.Empty<SdkWebSocket>();
            }

            var sockets = _sockets[envId];
            return sockets;
        }

        public async Task UnregisterSocket(SdkWebSocket socket)
        {
            var envId = socket.EnvId;

            if (!_sockets.ContainsKey(envId))
            {
                return;
            }

            var socketToRemove = _sockets[envId]?.FirstOrDefault(x => x.ConnectionId == socket.ConnectionId);
            if (socketToRemove != null)
            {
                await socketToRemove.CloseAsync();

                _sockets[envId].Remove(socketToRemove);
            }
        }
    }
}