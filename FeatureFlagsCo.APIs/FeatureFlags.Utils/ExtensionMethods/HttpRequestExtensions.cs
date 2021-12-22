using System;
using System.Linq;
using Microsoft.AspNetCore.Http;

namespace FeatureFlags.Utils.ExtensionMethods
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
        
        public static string AbsolutePath(this HttpRequest request)
        {
            var uriBuilder = new UriBuilder
            {
                Scheme = request.Scheme,
                Host = request.Host.Host,
                Path = request.Path.ToString(),
                Query = request.QueryString.ToString()
            };

            return uriBuilder.Uri.AbsolutePath;
        }
    }
}