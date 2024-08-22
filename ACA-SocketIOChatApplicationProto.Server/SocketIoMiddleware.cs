using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
namespace ACA_SocketIOChatApplicationProto.Server
{
    public class SocketIoMiddleware
    {
        private readonly RequestDelegate _next;

        public SocketIoMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            if (context.Request.Path.StartsWithSegments("/socket.io"))
            {
                var socket = await context.WebSockets.AcceptWebSocketAsync();
                await HandleSocketConnection(socket);
            }
            else
            {
                await _next(context);
            }
        }

        private async Task HandleSocketConnection(System.Net.WebSockets.WebSocket socket)
        {
            var buffer = new byte[1024 * 4];
            var result = await socket.ReceiveAsync(new ArraySegment<byte>(buffer), System.Threading.CancellationToken.None);

            while (!result.CloseStatus.HasValue)
            {
                await socket.SendAsync(new ArraySegment<byte>(buffer, 0, result.Count), result.MessageType, result.EndOfMessage, System.Threading.CancellationToken.None);
                result = await socket.ReceiveAsync(new ArraySegment<byte>(buffer), System.Threading.CancellationToken.None);
            }

            await socket.CloseAsync(result.CloseStatus.Value, result.CloseStatusDescription, System.Threading.CancellationToken.None);
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
