using System.Threading.Tasks;
using FeatureFlags.APIs.ViewModels.Analytic;
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
        [Route("analytics")]
        public async Task<Analytics> SaveAnalytics(CreateAnalyticsRequest input)
        {
            var analytics = input.Analytics(EnvId);

            var success = await _esService.IndexDocumentAsync(analytics, ElasticSearchIndices.Analytics);
            if (!success)
            {
                throw new ElasticSearchException("Failed to index int analytics, please check your elastic search log.");
            }

            return analytics;
        }
    }
}