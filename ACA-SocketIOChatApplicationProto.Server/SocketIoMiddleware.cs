using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ACA_SocketIOChatApplicationProto.Server
{
    public class SocketIoMiddleware
    {
        private readonly RequestDelegate _next;
        private static readonly ConcurrentDictionary<string, WebSocket> _connections = new();

        public SocketIoMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            if (context.Request.Path.StartsWithSegments("/socket.io"))
            {
                if (context.WebSockets.IsWebSocketRequest)
                {
                    var token = context.Request.Query["token"].ToString();  // Assuming token is passed in query string
                    var userId = JWTToken.GetUserIdFromToken(token);

                    if (string.IsNullOrEmpty(userId))
                    {
                        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                        await context.Response.WriteAsync("Unauthorized");
                        return;
                    }

                    var socket = await context.WebSockets.AcceptWebSocketAsync();
                    //socket.On("ImAlive", (data) =>
                    //{
                    //    Console.WriteLine("Received ImAlive event with data: " + data);
                    //    // Additional handling here
                    //});

                    StoreConnection(userId, socket);
                    await HandleSocketConnection(userId, socket);
                }               
            }
            else
            {
                await _next(context);
            }
        }

        private async Task HandleSocketConnection(string userId, WebSocket socket)
        {
            var buffer = new byte[1024 * 4];

            while (socket.State == WebSocketState.Open)
            {
                var result = await socket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

                if (result.MessageType == WebSocketMessageType.Close)
                {
                    break;
                }

                // Handle incoming message
                var message = Encoding.UTF8.GetString(buffer, 0, result.Count);

                // Process message and potentially send response
                await socket.SendAsync(new ArraySegment<byte>(Encoding.UTF8.GetBytes("response")), result.MessageType, result.EndOfMessage, CancellationToken.None);
            }

            _connections.TryRemove(userId, out _); // Clean up the connection after it is closed
            await socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Connection closed", CancellationToken.None);
        }

        private void StoreConnection(string userId, WebSocket socket)
        {
            _connections[userId] = socket;
        }

        private WebSocket GetConnection(string userId)
        {
            _connections.TryGetValue(userId, out var socket);
            return socket;
        }
    }

    public static class SocketIoExtensions
    {
        public static IApplicationBuilder UseSocketIo(this IApplicationBuilder app)
        {
            return app.UseMiddleware<SocketIoMiddleware>();
        }

        public static IApplicationBuilder MapSocketIoHandler(this IApplicationBuilder app, string path)
        {
            return app.Map(path, builder => builder.UseSocketIo());
        }
    }
}
