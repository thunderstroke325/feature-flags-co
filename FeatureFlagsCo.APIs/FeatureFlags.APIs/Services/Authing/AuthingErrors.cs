using System.Text.RegularExpressions;

namespace FeatureFlags.APIs.Services.Authing
{
    // https://docs.authing.cn/v2/reference/error-code.html
    public class AuthingErrors
    {
        // https://docs.authing.cn/v2/reference/error-code.html
        public const int EmailNotVerified = 2042;
        
        public static string Describe(string originMessage)
        {
            // see Authing.ApiClient.AuthingException
            var error = Regex.Replace(originMessage, "The API request failed.*:", "");
            return error.Trim();
        }
    }
}