using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FeatureFlags.APIs.Authentication;
using FeatureFlags.APIs.Models;
using FeatureFlags.APIs.Repositories;
using FeatureFlags.APIs.Services;
using FeatureFlags.APIs.ViewModels.Analytic;
using FeatureFlagsCo.MQ.ElasticSearch;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace FeatureFlags.APIs.Controllers.Public
{
    public class PublicAnalyticsController : PublicControllerBase
    {
        private readonly MongoDbAnalyticBoardService _mongoDb;
        private readonly ElasticSearchService _elasticSearch;
        private readonly ILogger<PublicAnalyticsController> _logger;
        private readonly IFeatureFlagsService _featureFlagService;

        public PublicAnalyticsController(
            ILogger<PublicAnalyticsController> logger,
            ElasticSearchService elasticSearch,
            IFeatureFlagsService featureFlagService,
            MongoDbAnalyticBoardService mongoDb)
        {
            _logger = logger;
            _elasticSearch = elasticSearch;
            _mongoDb = mongoDb;
            _featureFlagService = featureFlagService;
        }

        /// <summary>
        /// collect business analytics data
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

        /// <summary>
        /// collect feature flag usage data
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [Route("analytics/track/feature-flags")]
        public async Task<object> TrackFeatureFlagUsageAsync(List<FeatureFlagUsageParam> param)
        {
            if (param != null && param.Count > 0)
            {
                foreach (var item in param)
                {
                    if (!item.IsValid())
                    {
                        _logger.LogError(new Exception("Invalid param"), "Post /analytics/track/feature-flags ; param FeatureFlagUsage: " + JsonConvert.SerializeObject(item));
                    } 
                    else 
                    {
                        try
                        {
                            var ffIdVm = FeatureFlagKeyExtension.GetFeatureFlagIdByEnvironmentKey(EnvSecret, item.UserVariations[0].FeatureFlagKeyName);

                            item.UserVariations.ForEach(uv =>
                            {
                                _featureFlagService.SendFeatureFlagUsageToMQ(item, ffIdVm, uv);
                            });
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Post /analytics/track/feature-flags ; param FeatureFlagUsage: " + JsonConvert.SerializeObject(param));
                        }
                    }
                }
            }

            return StatusCode(StatusCodes.Status200OK, new Response { Code = "OK", Message = "OK" });
        }

        [HttpPost]
        [Route("analytics/userbehaviortrack")]
        public async Task<TrackUserBehaviorEvent> UserBehaviorTrackAsync(TrackUserBehaviorEventParam param)
        {
            string eventType = "";
            if (param.ClickEvent != null)
                eventType = "ClickEvent";
            if (param.CustomEvent != null)
                eventType = "CustomEvent";
            if (param.PageStayDurationEvent != null)
                eventType = "PageStayDurationEvent";
            if (param.PageViewEvent != null)
                eventType = "PageViewEvent";

            if(param.UtcTimeStampFromClientEnd == null)
            {
                throw new Exception("TimeStampFromClientEnd is empty");
            }
            var newEventObject = new TrackUserBehaviorEvent()
            {
                ClickEvent = param.ClickEvent,
                CustomEvent = param.CustomEvent,
                PageStayDurationEvent = param.PageStayDurationEvent,
                PageViewEvent = param.PageViewEvent,
                EnvironmentId = EnvId,
                TimeStamp = (long)(DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalMilliseconds,
                UserKey = param.UserKey,
                EventType = eventType,
                MediaType = MediaTypeEnum.WebAPP.ToString(),
                TimeStampFromClientEnd = param.UtcTimeStampFromClientEnd ?? 0
            };
            // index document
            var success = await _elasticSearch.IndexDocumentAsync(newEventObject, ElasticSearchIndices.UserBehaviorTrack);
            if (!success)
            {
                throw new ElasticSearchException("Failed to index analytics, please check your elastic search log.");
            }
            return newEventObject;
        }
    }
}