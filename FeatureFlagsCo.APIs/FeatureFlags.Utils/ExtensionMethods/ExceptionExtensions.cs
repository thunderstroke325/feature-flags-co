using System;
using System.Net;
using FeatureFlags.Utils.Exceptions;

namespace FeatureFlags.Utils.ExtensionMethods
{
    public static class ExceptionExtensions
    {
        public static HttpStatusCode ToHttpStatusCode(this Exception exception)
        {
            if (exception is ApiException apiException)
            {
                return apiException.HttpStatusCode;
            }
            
            if (exception is NotImplementedException)
            {
                return HttpStatusCode.NotImplemented;
            }

            if (exception is EntityNotFoundException)
            {
                return HttpStatusCode.NotFound;
            }

            if (exception is PermissionDeniedException || 
                exception is InvalidOperationException)
            {
                return HttpStatusCode.Forbidden;
            }

            if (exception is ArgumentException)
            {
                return HttpStatusCode.BadRequest;
            }

            return HttpStatusCode.InternalServerError;
        }
    }
}