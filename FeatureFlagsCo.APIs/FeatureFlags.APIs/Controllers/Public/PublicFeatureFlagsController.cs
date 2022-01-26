using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
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
        private readonly MongoDbFeatureFlagService _mongoDb;
        private readonly IMapper _mapper;

        public PublicFeatureFlagsController(
            IVariationService variationService,
            MongoDbFeatureFlagService mongoDb,
            IMapper mapper)
        {
            _variationService = variationService;
            _mongoDb = mongoDb;
            _mapper = mapper;
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

        /// <summary>
        /// 获取某个环境下所有未存档开关的所有信息
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("full-feature-flag")]
        public async Task<IEnumerable<FullFeatureFlagViewModel>> GetFullFeatureFlags()
        {
            var activeFeatureFlags = await _mongoDb.GetActiveFeatureFlags(EnvId);

            var vms = _mapper.Map<IEnumerable<FeatureFlag>, IEnumerable<FullFeatureFlagViewModel>>(activeFeatureFlags);
            return vms;
        }

        /// <summary>
        /// 获取某个环境下某个用户所有未存档开关的 variation 值
        /// </summary>
        /// <returns></returns>
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