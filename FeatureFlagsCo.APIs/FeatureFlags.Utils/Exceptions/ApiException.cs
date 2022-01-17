using System;
using System.Net;
using FeatureFlags.Utils.ExtensionMethods;

namespace FeatureFlags.Utils.Exceptions
{
    public class ApiException : Exception
    {
        public HttpStatusCode HttpStatusCode { get; }

        public ApiException(string message, Exception inner)
            : base(message, inner)
        {
            HttpStatusCode = inner.ToHttpStatusCode();
        }
    }
}