using Microsoft.AspNetCore.Builder;

namespace FeatureFlags.APIs.Middlewares
{
    public static class ApiExceptionMiddlewareExtension
    {
        public static IApplicationBuilder UseException(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<ApiExceptionMiddleware>();
        }
    }
}