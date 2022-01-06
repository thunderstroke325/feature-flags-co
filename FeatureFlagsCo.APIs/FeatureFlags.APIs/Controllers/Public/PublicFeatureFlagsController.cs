using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FeatureFlags.APIs.Repositories;
using FeatureFlags.APIs.Services;
using FeatureFlags.APIs.ViewModels.Public;
using Microsoft.AspNetCore.Mvc;

namespace FeatureFlags.APIs.Controllers.Public
{
    public class PublicFeatureFlagsController : PublicControllerBase
    {
        private readonly IVariationService _variationService;
        private readonly MongoDbFeatureFlagService _mongoDb;

        public PublicFeatureFlagsController(
            IVariationService variationService,
            MongoDbFeatureFlagService mongoDb)
        {
            _variationService = variationService;
            _mongoDb = mongoDb;
        }

        /// <summary>
        /// 获取某个环境下所有的未存档的 FeatureFlag
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("feature-flag")]
        public async Task<IEnumerable<FeatureFlagViewModel>> GetActiveFeatureFlags()
        {
            var activeFeatureFlags = await _mongoDb.GetActiveFeatureFlags(EnvId);

            return activeFeatureFlags.Select(f => new FeatureFlagViewModel { Id = f.Id, KeyName = f.FF.KeyName });
        }

        [HttpPost]
        [Route("feature-flag/variations")]
        public async Task<IEnumerable<UserVariationViewModel>> GetFeatureFlagVariations(GetUserVariationsRequest request)
        {
            var variations = new List<UserVariationViewModel>();
            
            var activeFeatureFlags = await _mongoDb.GetActiveFeatureFlags(EnvId);
            foreach (var featureFlag in activeFeatureFlags)
            {
                var ffIdVm =
                    FeatureFlagKeyExtension.GetFeatureFlagIdByEnvironmentKey(EnvSecret, featureFlag.FF.KeyName);

                var variation =
                    await _variationService.GetUserVariationAsync(EnvSecret, request.EnvironmentUser(), ffIdVm);
                
                variations.Add(new UserVariationViewModel
                {
                    Name = featureFlag.FF.Name,
                    KeyName = featureFlag.FF.KeyName,
                    Variation = variation.Variation.VariationValue
                });
            }

            return variations;
        }
    }
}