using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using FeatureFlags.APIs.Models;
using FeatureFlags.APIs.Services;
using FeatureFlags.APIs.Services.MongoDb;
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

        public PublicSdkController(
            FeatureFlagV2Service flagService, 
            IMapper mapper)
        {
            _flagService = flagService;
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
    }
}