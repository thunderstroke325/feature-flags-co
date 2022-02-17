using System;
using System.IO;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FeatureFlags.APIs.Models;
using FeatureFlags.APIs.Services;
using FeatureFlags.Utils.ExtensionMethods;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace FeatureFlags.APIs.Middlewares
{
    public class SdkWebSocketServerMiddleware
    {
        private const string SteamingPath = "/streaming";

        private readonly RequestDelegate _next;
        private readonly ILogger<SdkWebSocketServerMiddleware> _logger;

        public SdkWebSocketServerMiddleware(RequestDelegate next, ILogger<SdkWebSocketServerMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context, SdkWebSocketService wsService)
        {
            var request = context.Request;
            
            // if sdk websocket request
            if (!request.Path.StartsWithSegments(SteamingPath) || !context.WebSockets.IsWebSocketRequest)
            {
                await _next.Invoke(context);
                return;
            }

            // upgrade tcp connection to websocket connection
            using var webSocket = await context.WebSockets.AcceptWebSocketAsync();

            var sdkWebSocket = TryAcceptRequest(context);
            if (sdkWebSocket == null)
            {
                await webSocket.CloseAsync(
                    (WebSocketCloseStatus)4003,
                    "invalid request, close by server",
                    CancellationToken.None
                );
                return;
            }

            sdkWebSocket.AttachWebSocket(webSocket);

            await wsService.OnConnectedAsync(sdkWebSocket);

            await ReceiveAsync(sdkWebSocket, wsService);
        }

        private SdkWebSocket TryAcceptRequest(HttpContext context)
        {
            var query = context.Request.Query;

            // sdkType
            var sdkType = query["type"].ToString();
            if (!SdkTypes.IsRegistered(sdkType))
            {
                return null;
            }

            // parse token
            var token = query["token"].ToString();
            if (string.IsNullOrWhiteSpace(token) || !SdkWebSocketToken.TryCreate(token, out var socketToken))
            {
                return null;
            }

            // timestamp
            var curTimestamp = DateTime.UtcNow.UnixTimestampInMilliseconds();
            if (curTimestamp - socketToken.Timestamp > TimeSpan.FromSeconds(30).TotalMilliseconds)
            {
                return null;
            }

            var socket = new SdkWebSocket(socketToken.EnvSecret, sdkType);
            return socket;
        }

        private async Task ReceiveAsync(SdkWebSocket sdkWebSocket, SdkWebSocketService wsService)
        {
            var socket = sdkWebSocket.WebSocket;

            // receive & handle message
            while (socket.State == WebSocketState.Open)
            {
                var buffer = new ArraySegment<byte>(new byte[1024 * 4]);
                try
                {
                    string message;
                    WebSocketReceiveResult result;
                    await using (var ms = new MemoryStream())
                    {
                        do
                        {
                            result = await socket.ReceiveAsync(buffer, CancellationToken.None).ConfigureAwait(false);
                            if (buffer.Array != null)
                            {
                                ms.Write(buffer.Array, buffer.Offset, result.Count);
                            }
                        } while (!result.EndOfMessage);

                        ms.Seek(0, SeekOrigin.Begin);

                        using (var reader = new StreamReader(ms, Encoding.UTF8))
                        {
                            message = await reader.ReadToEndAsync().ConfigureAwait(false);
                        }
                    }

                    // handle text message only
                    if (result.MessageType == WebSocketMessageType.Text)
                    {
                        await wsService.OnMessageAsync(sdkWebSocket, message);
                    }
                }
                catch (WebSocketException e)
                {
                    if (e.WebSocketErrorCode == WebSocketError.ConnectionClosedPrematurely)
                    {
                        socket.Abort();
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, ex.Message);
                }
            }

            await wsService.OnDisconnectedAsync(sdkWebSocket);
        }
    }
}