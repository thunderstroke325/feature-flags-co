using System;
using System.Threading.Tasks;
using FeatureFlags.APIs.Controllers.Base;
using FeatureFlags.Common.ExtensionMethods;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace FeatureFlags.APIs.Middlewares
{
    public class ApiExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ApiExceptionMiddleware> _logger;

        public ApiExceptionMiddleware(RequestDelegate next, ILogger<ApiExceptionMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task Invoke(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);

                await context.Response.ReWriteAsync(
                    ex.ToHttpStatusCode(),
                    StandardApiResponse.Failed(ex.Message).SerializeToJson()
                );
            }
        }
    }
}