using Microsoft.AspNetCore.Builder;

namespace FeatureFlags.APIs.Middlewares
{
    public static class SdkWebSocketServerMiddlewareExtension
    {
        public static IApplicationBuilder UseSdkWebSocketServer(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<SdkWebSocketServerMiddleware>();
        }
    }
}