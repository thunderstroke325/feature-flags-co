using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using FeatureFlags.APIs.Models;
using FeatureFlags.APIs.Services;
using FeatureFlags.APIs.Services.MongoDb;
using FeatureFlags.APIs.ViewModels.Experiments;
using FeatureFlags.APIs.ViewModels.FeatureFlagZeroCodeSetting;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using MongoDB.Driver.Linq;

namespace FeatureFlags.APIs.Controllers.Public
{
    [Route("api/public/sdk")]
    public class PublicSdkController : PublicControllerBase
    {
        private readonly FeatureFlagV2Service _flagService;
        private readonly IMapper _mapper;
        private readonly MongoDbFeatureFlagZeroCodeSettingService _mongoDbFFZCSService;
        private readonly IExperimentsService _experimentsService;

        public PublicSdkController(
            FeatureFlagV2Service flagService, 
            IMapper mapper,
            MongoDbFeatureFlagZeroCodeSettingService mongoDbFFZCSService,
            IExperimentsService experimentsService)
        {
            _flagService = flagService;
            _mongoDbFFZCSService = mongoDbFFZCSService;
            _experimentsService = experimentsService;
            _mapper = mapper;
        }

        /// <summary>
        /// 获取某个环境下所有未存档开关的最新信息
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("latest-feature-flag")]
        public async Task<object> GetFullFeatureFlags()
        {
            var activeFeatureFlags = await _flagService.GetActiveFlagsAsync(EnvId);

            var sdkFlags = _mapper.Map<IEnumerable<FeatureFlag>, IEnumerable<ServerSdkFeatureFlag>>(activeFeatureFlags);

            return new
            {
                FeatureFlags = sdkFlags
            };
        }

        [HttpGet]
        [Route("zero-code")]
        public async Task<List<FeatureFlagZeroCodeSettingViewModel>> GetFeatureFlagHtmlDetectionSettings()
        {
            var allSettings = await _mongoDbFFZCSService.GetByEnvSecretAsync(EnvSecret);
            if (allSettings != null && allSettings.Count > 0)
            {
                var featureFlags = await _flagService.GetActiveByIdsAsync(allSettings.Select(s => s.FeatureFlagId));
                var ActiveFeatureFlagIds = featureFlags.Select(ff => ff.Id);

                return allSettings.Where(p => ActiveFeatureFlagIds.Contains(p.FeatureFlagId)).Select(p => new FeatureFlagZeroCodeSettingViewModel()
                {
                    Items = p.Items.Select(it => new CssSelectorItemViewModel { CssSelector = it.CssSelector, Url = it.Url, VariationValue = it.VariationOption.VariationValue, VariationOptionId = it.VariationOption.LocalId, HtmlContent = it.HtmlContent, HtmlProperties = it.HtmlProperties, Action = it.Action, Style = it.Style }).ToList(),
                    FeatureFlagKey = p.FeatureFlagKey,
                    FeatureFlagType = featureFlags.Find(ff => ff.Id == p.FeatureFlagId).FF.Type
                }).ToList();
            }

            return new List<FeatureFlagZeroCodeSettingViewModel>();
        }

        [HttpGet]
        [Route("experiments")]
        public async Task<IEnumerable<ExperimentMetricSetting>> GetActiveExperimentMetricSettings()
        {
            var secret = EnvironmentSecretV2.Parse(EnvSecret);
            return await _experimentsService.GetActiveExperimentMetricSettingsAsync(secret.EnvId);
        }
    }
}