using System;
using System.Net;

namespace FeatureFlags.Utils.ExtensionMethods
{
    public static class ExceptionExtensions
    {
        public static HttpStatusCode ToHttpStatusCode(this Exception exception)
        {
            if (exception is NotImplementedException)
            {
                return HttpStatusCode.NotImplemented;
            }
            
            return HttpStatusCode.InternalServerError;
        }
    }
}