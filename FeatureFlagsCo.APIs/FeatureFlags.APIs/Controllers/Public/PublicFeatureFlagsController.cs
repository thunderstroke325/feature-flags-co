using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FeatureFlags.APIs.Services;
using FeatureFlags.APIs.ViewModels.Public;
using Microsoft.AspNetCore.Mvc;

namespace FeatureFlags.APIs.Controllers.Public
{
    public class PublicFeatureFlagsController : PublicControllerBase
    {
        private readonly IEnvironmentService _environmentSrv;
        private readonly MongoDbFeatureFlagService _mongoDbFeatureFlagService;

        public PublicFeatureFlagsController(
            IEnvironmentService environmentSrv, 
            MongoDbFeatureFlagService mongoDbFeatureFlagService)
        {
            _environmentSrv = environmentSrv;
            _mongoDbFeatureFlagService = mongoDbFeatureFlagService;
        }
        
        /// <summary>
        /// 获取某个环境下所有的未存档的 FeatureFlag
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("feature-flag")]
        public async Task<IEnumerable<FeatureFlagViewModel>> GetActiveFeatureFlags()
        {
            var envId = await _environmentSrv.GetEnvIdBySecretAsync(EnvSecret);

            var activeFeatureFlags = await _mongoDbFeatureFlagService.GetActiveFeatureFlags(envId);
            return activeFeatureFlags.Select(f => new FeatureFlagViewModel { Id = f.Id, KeyName = f.FF.KeyName });
        }
    }
}