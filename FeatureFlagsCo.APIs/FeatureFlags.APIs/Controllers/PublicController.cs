using System.Collections.Generic;
using System.Threading.Tasks;
using FeatureFlags.APIs.Services;
using FeatureFlags.APIs.ViewModels.Public;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Linq;

namespace FeatureFlags.APIs.Controllers
{
    [AllowAnonymous]
    [ApiController]
    [Route("api/public/")]
    public class PublicController : ControllerBase
    {
        private readonly IEnvironmentService _environmentSrv;
        private readonly MongoDbFeatureFlagService _mongoDbFeatureFlagService;

        public PublicController(
            IEnvironmentService environmentSrv, 
            MongoDbFeatureFlagService mongoDbFeatureFlagService)
        {
            _environmentSrv = environmentSrv;
            _mongoDbFeatureFlagService = mongoDbFeatureFlagService;
        }

        /// <summary>
        /// 获取某个环境下所有的已存档的 FeatureFlag
        /// </summary>
        /// <param name="envSecret">环境 Secret</param>
        /// <returns></returns>
        [HttpGet]
        [Route("feature-flag")]
        public async Task<IEnumerable<FeatureFlagViewModel>> GetActiveFeatureFlags(string envSecret)
        {
            var envId = await _environmentSrv.GetEnvIdBySecretAsync(envSecret);

            var activeFeatureFlags = await _mongoDbFeatureFlagService.GetActiveFeatureFlags(envId);
            return activeFeatureFlags.Select(f => new FeatureFlagViewModel { Id = f.Id, KeyName = f.FF.KeyName });
        }
    }
}