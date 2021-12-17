using System.Linq;
using Microsoft.AspNetCore.Http;

namespace FeatureFlags.Common.ExtensionMethods
{
    public class RequestAuthKeys
    {
        public const string EnvSecret = "envSecret";
    }

    public static class HttpRequestExtensions
    {
        public static string EnvSecret(this HttpRequest request)
        {
            return request.AuthValueOf(RequestAuthKeys.EnvSecret);
        }
        
        public static string AuthValueOf(this HttpRequest request, string key)
        {
            var values = new[]
            {
                request.Query[key].ToString(),
                request.Headers[key].ToString(),
                request.Cookies[key]
            };
            
            var value = values.FirstOrDefault(x => !string.IsNullOrWhiteSpace(x));
            
            return value;
        }
    }
}