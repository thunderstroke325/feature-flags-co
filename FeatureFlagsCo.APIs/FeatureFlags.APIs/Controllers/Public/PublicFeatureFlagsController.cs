﻿using System.Collections.Generic;
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
        private readonly IVariationService _variationService;
        private readonly FeatureFlagV2Service _flagService;

        public PublicFeatureFlagsController(
            IVariationService variationService,
            FeatureFlagV2Service flagService)
        {
            _variationService = variationService;
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

            var activeFeatureFlags = await _flagService.GetActiveFlagsAsync(EnvId);
            foreach (var featureFlag in activeFeatureFlags)
            {
                var ffIdVm =
                    FeatureFlagKeyExtension.GetFeatureFlagIdByEnvironmentKey(EnvSecret, featureFlag.FF.KeyName);

                var variation =
                    await _variationService.GetUserVariationAsync(user.EnvironmentUser(), ffIdVm);

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
            
            var ffIdVm =
                FeatureFlagKeyExtension.GetFeatureFlagIdByEnvironmentKey(EnvSecret, request.FeatureFlagKeyName);

            var variation =
                await _variationService.GetUserVariationAsync(request.EnvironmentUser(), ffIdVm);

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