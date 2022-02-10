using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using FeatureFlags.APIs.Models;
using FeatureFlags.APIs.Services.MongoDb;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using MongoDB.Driver.Linq;

namespace FeatureFlags.APIs.Controllers.Public
{
    [Route("api/public/sdk")]
    public class PublicSdkController : PublicControllerBase
    {
        private readonly IMapper _mapper;
        private readonly MongoDbPersist _mongoDb;

        public PublicSdkController(IMapper mapper, MongoDbPersist mongoDb)
        {
            _mapper = mapper;
            _mongoDb = mongoDb;
        }

        /// <summary>
        /// 获取某个环境下所有未存档开关的最新信息
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("latest-feature-flag")]
        public async Task<object> GetFullFeatureFlags()
        {
            var activeFeatureFlags = await _mongoDb.QueryableOf<FeatureFlag>()
                .Where(featureFlag => featureFlag.EnvironmentId == EnvId && !featureFlag.IsArchived)
                .ToListAsync();

            var sdkFlags = _mapper.Map<IEnumerable<FeatureFlag>, IEnumerable<ServerSdkFeatureFlag>>(activeFeatureFlags);

            return new
            {
                FeatureFlags = sdkFlags
            };
        }
    }
}