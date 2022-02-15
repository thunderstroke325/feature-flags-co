using System;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace FeatureFlags.APIs.Models
{
    public class SdkWebSocket
    {
        public int EnvId { get; protected set; }

        public string EnvSecret { get; protected set; }

        public string ConnectionId { get; protected set; }

        public WebSocket WebSocket { get; protected set; }

        public string SdkType { get; protected set; }

        /// <summary>
        /// client-side sdk use only
        /// </summary>
        [CanBeNull]
        public FeatureFlagUser User { get; set; }

        public SdkWebSocket(string envSecret, string sdkType)
        {
            ConnectionId = Guid.NewGuid().ToString("D");

            if (!EnvironmentSecretV2.TryParse(envSecret, out var secret))
            {
                throw new ArgumentException($"invalid envSecret {envSecret}", nameof(envSecret));
            }

            EnvId = secret.EnvId;
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

        /// <summary>
        /// client-side sdk use only
        /// </summary>
        public void AttachUser(FeatureFlagUser user)
        {
            User = user;
        }

        public async Task SendAsync(SdkWebSocketMessage message)
        {
            var bytes = Encoding.UTF8.GetBytes(message.AsJson());
            var buffer = new ArraySegment<byte>(bytes, 0, bytes.Length);

            await WebSocket.SendAsync(buffer, WebSocketMessageType.Text, true, CancellationToken.None);
        }

        public async Task CloseAsync()
        {
            var closeStatus = WebSocket.CloseStatus; 
            if (closeStatus.HasValue)
            {
                await WebSocket.CloseAsync(
                    closeStatus.Value,
                    WebSocket.CloseStatusDescription, 
                    CancellationToken.None
                );
            }
        }

        public override string ToString()
        {
            var info =
                $"connectionId: {ConnectionId}, envSecret: {EnvSecret}, sdkType: {SdkType}" +
                $"{(User == null ? "" : User.UserKeyId)}";

            return info;
        }
    }
}