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
            _logger = logger;
            var connectionString = configuration.GetSection("MySettings").GetSection("ElasticSearchHost").Value;
            
            var (auth, apiPrefix) = ParseConnectionString(connectionString);
            _apiPrefix = apiPrefix;
            
            client.DefaultRequestHeaders.Accept.TryParseAdd("application/json");
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
                "Basic", Convert.ToBase64String(Encoding.ASCII.GetBytes(auth))
            );
            _client = client;
        }

        /// <summary>
        /// index a document in elastic search
        /// </summary>
        /// <param name="index">the es index</param>
        /// <param name="jsonContent">the content in json format</param>
        /// <returns></returns>
        /// <a>https://www.elastic.co/guide/en/elasticsearch/reference/current/docs-index_.html</a>
        public async Task CreateDocumentAsync(string index, string jsonContent)
        {
            var url = $"{_apiPrefix}/{index}/_doc/";
            var body = new StringContent(jsonContent, Encoding.UTF8, MediaTypeNames.Application.Json);
            
            try
            {
                var response = await _client.PostAsync(url, body);
                if (response.StatusCode != HttpStatusCode.Created)
                {
                    throw new HttpRequestException($"Failed to create document. Status code: {response.StatusCode}");
                }
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, $"POST {url} WITH {jsonContent}.");
            }
        }

        private (string auth, string apiPrefix) ParseConnectionString(string connectionString)
        {
            var authStart = connectionString.LastIndexOf("/", StringComparison.Ordinal) + 1;
            var authEnd = connectionString.LastIndexOf("@", StringComparison.Ordinal);
            var auth = connectionString.Substring(authStart, authEnd - authStart);
            var hostAndPort = connectionString.Remove(authStart, authEnd - authStart + 1);
            
            return (auth, hostAndPort);
        }
    }
}