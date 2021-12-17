using System.Threading.Tasks;
using FeatureFlagsCo.MQ.ElasticSearch;
using FeatureFlagsCo.MQ.ElasticSearch.DataModels;
using Microsoft.AspNetCore.Mvc;

namespace FeatureFlags.APIs.Controllers.Public
{
    public class PublicAnalyticBoardController : PublicControllerBase
    {
        private readonly ElasticSearchService _esService;

        public PublicAnalyticBoardController(ElasticSearchService esService)
        {
            _esService = esService;
        }

        [HttpPost]
        [Route("analytic-board/int-analytics")]
        public async Task<IntAnalytics> SaveIntAnalytics(CreateIntAnalyticsRequest input)
        {
            var intAnalytics = input.IntAnalytics(EnvId);

            var success = await _esService.IndexDocumentAsync(intAnalytics, ElasticSearchIndices.Analytics);
            if (!success)
            {
                throw new ElasticSearchException("Failed to index int analytics, please check your elastic search log.");
            }

            return intAnalytics;
        }
    }
}