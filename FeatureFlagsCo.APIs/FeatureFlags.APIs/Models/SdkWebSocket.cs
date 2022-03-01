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

        public DateTime ConnectAt { get; set; }

        public DateTime? DisConnectAt { get; set; }

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
            
            if (!SdkTypes.IsRegistered(sdkType))
            {
                throw new ArgumentException($"sdkType {sdkType} is not registered", nameof(sdkType));
            }
            SdkType = sdkType;
            
            ConnectAt = DateTime.UtcNow;
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

        public async Task CloseSafelyAsync()
        {
            var closeStatus = WebSocket.CloseStatus;
            if (closeStatus.HasValue)
            {
                try
                {
                    await WebSocket.CloseAsync(
                        closeStatus.Value,
                        WebSocket.CloseStatusDescription,
                        CancellationToken.None
                    );
                }
                catch (WebSocketException ex) when (ex.WebSocketErrorCode == WebSocketError.ConnectionClosedPrematurely)
                {
                    WebSocket.Abort();
                }
                catch (Exception)
                {
                    // just ignore other exception
                }
            }
            
            DisConnectAt = DateTime.UtcNow;
        }

        public override string ToString()
        {
            var connectionTimeInfo = DisConnectAt.HasValue
                ? $"[{ConnectAt:yyyy-MM-ddTHH:mm:ss.fff}, {DisConnectAt:yyyy-MM-ddTHH:mm:ss.fff}]"
                : $"[{ConnectAt:yyyy-MM-dd HH:mm:ss.fff}, -]";

            var info =
                $"connectionId: {ConnectionId}, connectionTime: {connectionTimeInfo} envSecret: {EnvSecret}, sdkType: {SdkType}" +
                $"{(User == null ? "" : $" UserKeyId: {User.UserKeyId}")}";

            return info;
        }
    }
}