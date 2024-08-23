using Microsoft.AspNetCore.Http;
using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Security.Claims;
using System.Text;
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
        // Only needed if using a WebSocket
        if (context.WebSockets.IsWebSocketRequest)
        {
            var socket = await context.WebSockets.AcceptWebSocketAsync();
            await HandleSocketConnection(socket);
        }
        else
        {
            // For Socket.IO, you need to access the query parameters for authentication
            var token = context.Request.Query["token"].FirstOrDefault();
            
            if (!string.IsNullOrEmpty(token))
            {
                var userId = JWTToken.GetUserIdFromToken(token);
                if (userId != null)
                {
                    // Handle the connection and store it
                    var socket = await context.WebSockets.AcceptWebSocketAsync();
                    StoreConnection(userId, socket);
                    await HandleSocketConnection(socket);
                }
                else
                {
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    await context.Response.WriteAsync("Unauthorized");
                }
            }
            else
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                await context.Response.WriteAsync("Unauthorized");
            }
        }
    }
    else
    {
        await _next(context);
    }
}

        private async Task HandleSocketConnection(System.Net.WebSockets.WebSocket socket)
        {
            var buffer = new byte[1024 * 4];
            var result = await socket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

            while (!result.CloseStatus.HasValue)
            {
                // Handle incoming message
                var message = Encoding.UTF8.GetString(buffer, 0, result.Count);
                // Process message and potentially send response
                await socket.SendAsync(new ArraySegment<byte>(Encoding.UTF8.GetBytes("response")), result.MessageType, result.EndOfMessage, CancellationToken.None);

                result = await socket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
            }

            await socket.CloseAsync(result.CloseStatus.Value, result.CloseStatusDescription, CancellationToken.None);
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
