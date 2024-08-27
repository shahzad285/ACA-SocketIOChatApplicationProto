namespace ACA_SocketIOChatApplicationProto.Server
{
    public class AuthenticationMiddleware
    {
        private readonly RequestDelegate _next;

        public AuthenticationMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext
     context)
        {
            context.Request.Headers.TryGetValue("Authorization", out var token);
            if (!string.IsNullOrEmpty(token))
            {
                var userId = JWTToken.GetUserIdFromToken(token);
                if (userId != null)
                {
                    context.Items["UserId"] = userId; // Store user ID for later access
                }
            }
            await _next(context);
        }
    }
}
