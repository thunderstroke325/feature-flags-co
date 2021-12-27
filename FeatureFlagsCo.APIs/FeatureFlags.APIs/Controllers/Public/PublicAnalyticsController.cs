using System.Threading.Tasks;
using FeatureFlags.APIs.Services;
using FeatureFlags.APIs.ViewModels.Analytic;
using FeatureFlagsCo.MQ.ElasticSearch;
using Microsoft.AspNetCore.Mvc;

namespace FeatureFlags.APIs.Controllers.Public
{
    public class PublicAnalyticsController : PublicControllerBase
    {
        private readonly MongoDbAnalyticBoardService _mongoDb;
        private readonly ElasticSearchService _elasticSearch;

        public PublicAnalyticsController(
            ElasticSearchService elasticSearch, 
            MongoDbAnalyticBoardService mongoDb)
        {
            _elasticSearch = elasticSearch;
            _mongoDb = mongoDb;
        }

        /// <summary>
        /// 数据报表数据收集
        /// </summary>
        /// <returns></returns>
        /// <exception cref="ElasticSearchException"></exception>
        [HttpPost]
        [Route("analytics/track")]
        public async Task TrackAsync(CreateAnalyticsRequest input)
        {
            var analytics = input.Analytics(EnvId);

            // index document
            var success = await _elasticSearch.IndexDocumentAsync(analytics, ElasticSearchIndices.Analytics);
            if (!success)
            {
                throw new ElasticSearchException("Failed to index analytics, please check your elastic search log.");
            }
            
            // try add new dimension
            var board = await _mongoDb.GetByEnvIdAsync(EnvId);
            if (board != null)
            {
                foreach (var inputDimension in input.Dimensions)
                {
                    board.TryAddDimension(inputDimension.Key, inputDimension.Value);
                }

                await _mongoDb.UpdateAsync(board.Id, board);
            }
        }
    }
}