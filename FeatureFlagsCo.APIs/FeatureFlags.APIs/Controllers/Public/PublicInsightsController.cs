using AutoMapper;
using FeatureFlags.APIs.Authentication;
using FeatureFlags.APIs.Models;
using FeatureFlags.APIs.Repositories;
using FeatureFlags.APIs.Services;
using FeatureFlagsCo.MQ;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FeatureFlags.APIs.Controllers.Public
{
    public class PublicInsightsController: PublicControllerBase
    {
        private readonly ILogger<PublicInsightsController> _logger;
        private readonly IMapper _mapper;
        private readonly MessagingService _messagingService;

        public PublicInsightsController(
            ILogger<PublicInsightsController> logger,
            MessagingService messagingService,
            IMapper mapper)
        {
            _logger = logger;
            _mapper = mapper;
            _messagingService = messagingService;
        }


        /// <summary>
        /// collect feature flag usage data
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [Route("track")]
        public async Task<object> TrackAsync(
            [FromServices] IFeatureFlagsService featureFlagService,
            [FromServices] EnvironmentUserV2Service envUserService,
            List<InsightParam> param)
        {
            if (param == null || param.Count <= 0)
            {
                return StatusCode(StatusCodes.Status200OK, new Response { Code = "OK", Message = "OK" });
            }

            foreach (var item in param)
            {
                if (!item.IsValid())
                {
                    _logger.LogError(new Exception("Invalid param"), "Post /analytics/track/feature-flags ; param FeatureFlagUsage: " + JsonConvert.SerializeObject(item));
                    continue;
                }

                if (item.UserVariations != null && item.UserVariations.Count > 0) 
                {
                    try
                    {
                        // upsert environment user
                        var envUser = item.AsEnvironmentUser(EnvId);
                        var fireAndForget = envUserService.UpsertAsync(envUser);

                        var ffIdVm = FeatureFlagKeyExtension.GetFeatureFlagIdByEnvironmentKey(EnvSecret, item.UserVariations[0].FeatureFlagKeyName);

                        item.UserVariations.ForEach(uv =>
                        {
                            featureFlagService.SendFeatureFlagUsageToMQ(item, ffIdVm, uv);
                        });
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Post /insights/track; param FeatureFlagUsage: " + JsonConvert.SerializeObject(item));
                    }
                }

                if (item.Metrics != null && item.Metrics.Count > 0)
                {
                    try
                    {
                        var ffIdVM = FeatureFlagKeyExtension.GetEnvIdsByEnvKey(EnvSecret);

                        item.Metrics.ForEach(m => 
                        {
                            var message = new ExperimentMessageModel
                            {
                                Route = m.Route,
                                Secret = EnvSecret,
                                Type = m.Type,
                                EventName = m.EventName,
                                NumericValue = m.NumericValue,
                                User = _mapper.Map<InsightUser, MqUserInfo>(item.User),
                                AppType = m.AppType,
                                CustomizedProperties = _mapper.Map<List<CustomizedProperty>, List<MqCustomizedProperty>>(item.CustomizedProperties ?? new List<CustomizedProperty>()),
                                ProjectId = ffIdVM.ProjectId,
                                EnvironmentId = ffIdVM.EnvId,
                                AccountId = ffIdVM.AccountId,
                                TimeStamp = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.ffffff")
                            };

                            _messagingService.SendEventDataWithoutResponse(message);
                        });
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Post /insights/track; param event: " + JsonConvert.SerializeObject(item));
                    }
                }
            }

            return StatusCode(StatusCodes.Status200OK, new Response { Code = "OK", Message = "OK" });
        }
    }
}
