using System;
using System.Text.Json;
using System.Threading.Tasks;
using FeatureFlags.APIs.Controllers.Base;
using FeatureFlags.Common.ExtensionMethods;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace FeatureFlags.APIs.MvcFilters
{
    public class ApiException : Exception
    {
        public ApiException()
        {
        }

        public ApiException(string message)
            : base(message)
        {
        }

        public ApiException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }

    public class ApiActionFilterAttribute : ActionFilterAttribute
    {
        public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            // execute action
            var resultContext = await next();

            // handle any unhandled action exception
            var exception = resultContext.Exception;
            if (exception != null && !resultContext.ExceptionHandled)
            {
                var request = context.HttpContext.Request;
                var parameters = JsonSerializer.Serialize(context.ActionArguments);
                var message =
                    $"{request.Method} {request.AbsolutePath()} with parameter {parameters} error: {exception.Message}";

                resultContext.ExceptionHandled = true;

                throw new ApiException(message, resultContext.Exception);
            }

            // after action execution
            if (resultContext.Result is ObjectResult result)
            {
                resultContext.Result = new JsonResult(
                    new StandardApiResponse(
                        true,
                        result.Value,
                        "api call success"
                    )
                );
            }
        }
    }
}