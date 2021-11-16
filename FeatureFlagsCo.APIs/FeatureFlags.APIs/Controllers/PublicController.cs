using System.Collections.Generic;
using System.Threading.Tasks;
using FeatureFlags.APIs.Models;
using FeatureFlags.APIs.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FeatureFlags.APIs.Controllers
{
    [AllowAnonymous]
    [ApiController]
    [Route("public/api/")]
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
        [Route("feature-flag/archived")]
        public async Task<List<FeatureFlag>> GetArchivedFeatureFlags(string envSecret)
        {
            var envId = await _environmentSrv.GetEnvIdBySecretAsync(envSecret);

            var archivedFeatureFlags = await _mongoDbFeatureFlagService.GetArchivedFeatureFlags(envId);
            return archivedFeatureFlags;
        }
    }
}