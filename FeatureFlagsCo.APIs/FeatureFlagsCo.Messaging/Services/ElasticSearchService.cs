using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace FeatureFlagsCo.Messaging.Services
{
    /// <summary>
    /// ElasticSearch Typed Client
    /// </summary>
    public class ElasticSearchService
    {
        private readonly ILogger<ElasticSearchService> _logger;
        private readonly string _apiPrefix;
        private readonly HttpClient _client;

        public ElasticSearchService(
            HttpClient client,
            ILogger<ElasticSearchService> logger,
            IConfiguration configuration)
        {
            var connectionString = configuration.GetSection("MySettings:ElasticSearchHost").Value;
            var (auth, hostAndPort) = ParseSpecificConnectionString(connectionString);

            _apiPrefix = hostAndPort;

            // add default headers
            client.DefaultRequestHeaders.Accept.TryParseAdd(MediaTypeNames.Application.Json);
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
                "Basic", Convert.ToBase64String(Encoding.ASCII.GetBytes(auth))
            );
            _client = client;

            _logger = logger;
        }

        /// <summary>
        /// index a document in elastic search
        /// </summary>
        /// <param name="index">the es index</param>
        /// <param name="jsonContent">the content in json format</param>
        /// <returns></returns>
        /// <a>https://www.elastic.co/guide/en/elasticsearch/reference/current/docs-index_.html</a>
        public async Task<bool> CreateDocumentAsync(string index, string jsonContent)
        {
            var url = $"{_apiPrefix}/{index}/_doc/";
            var body = new StringContent(jsonContent, Encoding.UTF8, MediaTypeNames.Application.Json);

            var createSuccess = false;
            try
            {
                var response = await _client.PostAsync(url, body);
                if (response.StatusCode == HttpStatusCode.Created)
                {
                    createSuccess = true;
                }
                else
                {
                    throw new HttpRequestException($"Failed to create document. Status code: {response.StatusCode}");
                }
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, $"POST {url} WITH {jsonContent}.");
            }

            return createSuccess;
        }

        /// <summary>
        /// parse the es **specific** connection string to get the auth and host
        /// </summary>
        /// <param name="connectionString"></param>
        /// <returns></returns>
        /// <example>
        /// GIVEN:  https://username:password@host:port
        /// RETURN: ("username:password", "https://host:port")
        /// </example>
        private (string auth, string hostAndPort) ParseSpecificConnectionString(string connectionString)
        {
            var authStart = connectionString.LastIndexOf("/", StringComparison.Ordinal) + 1;
            var authEnd = connectionString.LastIndexOf("@", StringComparison.Ordinal);
            
            var auth = connectionString.Substring(authStart, authEnd - authStart);
            var hostAndPort = connectionString.Remove(authStart, authEnd - authStart + 1);

            return (auth, hostAndPort);
        }
    }
}