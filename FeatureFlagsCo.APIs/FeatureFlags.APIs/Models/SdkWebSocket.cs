using System;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FeatureFlags.APIs.Models
{
    public class SdkWebSocket
    {
        public string EnvSecret { get; protected set; }

        public string ConnectionId { get; protected set; }

        public WebSocket WebSocket { get; protected set; }

        public string SdkType { get; protected set; }

        public SdkWebSocket(string envSecret, string sdkType)
        {
            ConnectionId = Guid.NewGuid().ToString("D");

            if (!EnvironmentSecretV2.TryParse(envSecret, out _))
            {
                throw new ArgumentException($"invalid envSecret {envSecret}", nameof(envSecret));
            }

            EnvSecret = envSecret;

            if (string.IsNullOrWhiteSpace(sdkType))
            {
                throw new ArgumentException("sdkType cannot be null or whitespace", nameof(sdkType));
            }

            if (!SdkTypes.IsRegistered(sdkType))
            {
                throw new ArgumentException($"sdkType {sdkType} is not registered", nameof(sdkType));
            }

            SdkType = sdkType;
        }

        public void AttachWebSocket(WebSocket webSocket)
        {
            if (webSocket == null)
            {
                throw new ArgumentNullException(nameof(webSocket));
            }

            if (!new[] { WebSocketState.Open, WebSocketState.Connecting }.Contains(webSocket.State))
            {
                throw new ArgumentException("websocket must be in the Open or Connecting state", nameof(webSocket));
            }

            WebSocket = webSocket;
        }

        public async Task SendAsync(SdkWebSocketMessage message)
        {
            var bytes = Encoding.UTF8.GetBytes(message.AsJson());
            var buffer = new ArraySegment<byte>(bytes, 0, bytes.Length);

            await WebSocket.SendAsync(buffer, WebSocketMessageType.Text, true, CancellationToken.None);
        }

        public async Task CloseAsync()
        {
            if (WebSocket.State == WebSocketState.CloseReceived)
            {
                await WebSocket.CloseAsync(
                    WebSocketCloseStatus.NormalClosure,
                    "server receive close request, close by client", CancellationToken.None
                );
            }
        }

        public override string ToString()
        {
            var info =
                $"connectionId: {ConnectionId}, envSecret: {EnvSecret}, sdkType: {SdkType}";

            return info;
        }
    }
}