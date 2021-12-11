using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Nest;

namespace FeatureFlagsCo.MQ.ElasticSearch
{
    /// <summary>
    /// ElasticSearch Typed Client
    /// </summary>
    public class ElasticSearchService
    {
        private readonly ElasticClient _client;
        private readonly ILogger<ElasticSearchService> _logger;

        public ElasticSearchService(ElasticClient client, ILogger<ElasticSearchService> logger)
        {
            _client = client;
            _logger = logger;
        }

        /// <summary>
        /// index a document
        /// </summary>
        /// <param name="document">the object to index</param>
        /// <param name="indexName">the index name</param>
        /// <typeparam name="TDocument">document type</typeparam>
        /// <returns>if this operation success</returns>
        /// <a>https://www.elastic.co/guide/en/elasticsearch/reference/current/docs-index_.html</a>
        public async Task<bool> IndexDocumentAsync<TDocument>(TDocument document, string indexName)
            where TDocument : class
        {
            var response = await _client.IndexAsync(document, descriptor => descriptor.Index(indexName));
            if (!response.IsValid)
            {
                var jsonDocument = JsonSerializer.Serialize(document);

                _logger.LogError(
                    $"Failed to index document {jsonDocument} in index {indexName}. Debug Information: " +
                    $"Api call is {response.ApiCall}, Server Error is {response.ServerError}"
                );
            }

            return response.IsValid;
        }

        /// <summary>
        /// count documents by filter
        /// </summary>
        /// <param name="countDescriptor">count descriptor for the document</param>
        /// <typeparam name="TDocument">document type</typeparam>
        /// <returns></returns>
        public async Task<long> CountDocumentsAsync<TDocument>(CountDescriptor<TDocument> countDescriptor)
            where TDocument : class
        {
            var countResponse = await _client.CountAsync(countDescriptor);
            if (!countResponse.IsValid)
            {
                _logger.LogError(
                    "Failed to count documents. Debug Information: " +
                    $"Api call is {countResponse.ApiCall}, Server Error is {countResponse.ServerError}"
                );

                return 0;
            }

            return countResponse.Count;
        }
        
        /// <summary>
        /// search or analyse documents
        /// </summary>
        /// <param name="searchDescriptor">search descriptor for the document</param>
        /// <typeparam name="TDocument"></typeparam>
        /// <returns></returns>
        public async Task<ISearchResponse<TDocument>> SearchDocumentAsync<TDocument>(SearchDescriptor<TDocument> searchDescriptor)
            where TDocument : class
        {
            var response = await _client.SearchAsync<TDocument>(searchDescriptor);
            if (!response.IsValid)
            {
                _logger.LogError(
                    "Failed to get average. Debug Information: " +
                    $"Api call is {response.ApiCall}, Server Error is {response.ServerError}"
                );
            }

            return response;
        }
    }
}