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
        // envSecret -> SdkWebSocket[], SdkWebSocket GroupBy envId
        private readonly ConcurrentDictionary<string, List<SdkWebSocket>> _sockets;

        public SdkWebSocketConnectionManager()
        {
            _sockets = new ConcurrentDictionary<string, List<SdkWebSocket>>();
        }

        public void RegisterSocket(SdkWebSocket socket)
        {
            var envSecret = socket.EnvSecret;

            if (_sockets.ContainsKey(envSecret))
            {
                _sockets[envSecret].Add(socket);
            }
            else
            {
                _sockets[envSecret] = new List<SdkWebSocket> { socket };
            }
        }

        public SdkWebSocket GetSocket(string connectionId)
        {
            var socket = _sockets.Values.SelectMany(x => x).FirstOrDefault(x => x.ConnectionId == connectionId);

            return socket;
        }

        public IEnumerable<SdkWebSocket> GetSockets(string envSecret)
        {
            if (!_sockets.ContainsKey(envSecret))
            {
                return Array.Empty<SdkWebSocket>();
            }

            var sockets = _sockets[envSecret];
            return sockets;
        }

        public async Task UnregisterSocket(SdkWebSocket socket)
        {
            var envSecret = socket.EnvSecret;

            if (!_sockets.ContainsKey(envSecret))
            {
                return;
            }

            var socketToRemove = _sockets[envSecret].FirstOrDefault(x => x.ConnectionId == socket.ConnectionId);
            if (socketToRemove != null)
            {
                await socketToRemove.CloseAsync();

                _sockets[envSecret].Remove(socketToRemove);
            }
        }
    }
}