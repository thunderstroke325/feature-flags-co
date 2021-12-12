using System.Net;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace FeatureFlags.Common.ExtensionMethods
{
    public static class HttpResponseExtensions
    {
        public static async Task ReWriteAsync(
            this HttpResponse response, 
            HttpStatusCode statusCode, 
            string content, 
            string contentType = "application/json; charset=UTF-8")
        {
            response.Clear();
            response.StatusCode = (int) statusCode;
            response.ContentType = contentType;

            await response.WriteAsync(content, Encoding.UTF8);
        }
    }
}