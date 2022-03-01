using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FeatureFlags.APIs.Models;
using FeatureFlags.APIs.Repositories;
using FeatureFlags.APIs.Services;
using FeatureFlags.APIs.ViewModels.Public;
using Microsoft.AspNetCore.Mvc;

namespace FeatureFlags.APIs.Controllers.Public
{
    public class PublicFeatureFlagsController : PublicControllerBase
    {
        private readonly VariationService _variationService;
        private readonly EnvironmentUserV2Service _envUserService;
        private readonly FeatureFlagV2Service _flagService;

        public PublicFeatureFlagsController(
            VariationService variationService,
            EnvironmentUserV2Service envUserService, 
            FeatureFlagV2Service flagService)
        {
            _variationService = variationService;
            _envUserService = envUserService;
            _flagService = flagService;
        }

        /// <summary>
        /// 获取某个环境下所有的未存档的 FeatureFlag
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("feature-flag")]
        public async Task<IEnumerable<FeatureFlagViewModel>> GetActiveFeatureFlags()
        {
            var activeFeatureFlags = await _flagService.GetActiveFlagsAsync(EnvId);

            return activeFeatureFlags.Select(f => new FeatureFlagViewModel { Id = f.Id, KeyName = f.FF.KeyName });
        }

        /// <summary>
        /// 获取某个环境下某个用户所有未存档开关的 variation 值
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [Route("feature-flag/variations")]
        public async Task<IEnumerable<UserVariationViewModel>> GetFeatureFlagVariations(FeatureFlagUser user)
        {
            var variations = new List<UserVariationViewModel>();

            // upsert environment user
            var envUser = user.AsEnvironmentUser(EnvId);
            await _envUserService.UpsertAsync(envUser);
            
            var activeFeatureFlags = await _flagService.GetActiveFlagsAsync(EnvId);
            foreach (var featureFlag in activeFeatureFlags)
            {
                var variation = await _variationService.GetVariationAsync(EnvSecret, featureFlag.FF.KeyName, envUser);

                variations.Add(new UserVariationViewModel
                {
                    Id = variation.Variation.LocalId, 
                    Name = featureFlag.FF.Name,
                    KeyName = featureFlag.FF.KeyName,
                    Variation = variation.Variation.VariationValue, 
                    Reason = string.Empty
                });
            }

            return variations;
        }
        
        [HttpPost]
        [Route("feature-flag/variation")]
        public async Task<UserVariationViewModel> GetFeatureFlagVariation(FeatureFlagUserVariationRequest request)
        {
            var featureFlag = await _flagService.FindAsync(flag => flag.FF.KeyName == request.FeatureFlagKeyName);
            if (featureFlag == null)
            {
                return null;
            }
            
            // upsert environment user
            var envUser = request.AsEnvironmentUser(EnvId);
            await _envUserService.UpsertAsync(envUser);
            
            var variation = await _variationService.GetVariationAsync(EnvSecret, featureFlag.FF.KeyName, envUser);
            
            return new UserVariationViewModel
            {
                Id = variation.Variation.LocalId, 
                Name = featureFlag.FF.Name,
                KeyName = featureFlag.FF.KeyName,
                Variation = variation.Variation.VariationValue, 
                Reason = string.Empty
            };
        }
    }
}