using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FeatureFlags.APIs.Models;
using FeatureFlags.APIs.Repositories;
using FeatureFlags.APIs.Services;
using FeatureFlags.APIs.ViewModels.FeatureFlagsViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace FeatureFlags.APIs.Controllers
{
    //[Authorize(Roles = UserRoles.Admin)]
    [Authorize]
    [ApiController]
    [Route("[controller]")]
    public class FeatureFlagsController : ControllerBase
    {
        private readonly IGenericRepository _repository;
        private readonly ILogger<FeatureFlagsController> _logger;
        private readonly IFeatureFlagsService _featureFlagService;
        private readonly ICosmosDbService _cosmosDbService;
        private readonly IDistributedCache _redisCache;

        public FeatureFlagsController(ILogger<FeatureFlagsController> logger, IGenericRepository repository,
            IFeatureFlagsService featureFlagService,
            ICosmosDbService cosmosDbService,
            IDistributedCache redisCache)
        {
            _logger = logger;
            _repository = repository;
            _featureFlagService = featureFlagService;
            _cosmosDbService = cosmosDbService;
            _redisCache = redisCache;
        }


        [HttpGet]
        [Route("GetEnvironmentFeatureFlags/{environmentId}")]
        public async Task<List<CosmosDBFeatureFlagBasicInfo>> GetEnvironmentFeatureFlags(int environmentId)
        {
            return await _cosmosDbService.GetEnvironmentFeatureFlagBasicInfoItemsAsync(environmentId, 0, 300);
        }

        [HttpGet]
        [Route("GetEnvironmentArchivedFeatureFlags/{environmentId}")]
        public async Task<List<CosmosDBFeatureFlagBasicInfo>> GetEnvironmentArchivedFeatureFlags(int environmentId)
        {
            return await _cosmosDbService.GetEnvironmentArchivedFeatureFlagBasicInfoItemsAsync(environmentId, 0, 300);
        }

        [HttpPost]
        [Route("ArchiveEnvironmentdFeatureFlag")]
        public async Task<CosmosDBFeatureFlag> ArchiveEnvironmentdFeatureFlag([FromBody] FeatureFlagArchiveParam param)
        {
            await _redisCache.RemoveAsync(param.FeatureFlagId);
            return await _cosmosDbService.ArchiveEnvironmentdFeatureFlagAsync(param);
        }

        [HttpPost]
        [Route("UnarchiveEnvironmentdFeatureFlag")]
        public async Task<CosmosDBFeatureFlag> UnarchiveEnvironmentdFeatureFlag([FromBody] FeatureFlagArchiveParam param)
        {
            await _redisCache.RemoveAsync(param.FeatureFlagId);
            return await _cosmosDbService.UnarchiveEnvironmentdFeatureFlagAsync(param);
        }

        [HttpPost]
        [Route("SwitchFeatureFlag")]
        public async Task SwitchFeatureFlag([FromBody] CosmosDBFeatureFlagBasicInfo param)
        {
            CosmosDBFeatureFlag ff = await _cosmosDbService.GetFeatureFlagAsync(param.Id);
            ff.FF.LastUpdatedTime = DateTime.UtcNow;
            ff.FF.Status = param.Status;
            var updatedFeatureFalg = await _cosmosDbService.UpdateCosmosDBFeatureFlagAsync(ff);
            await _redisCache.SetStringAsync(updatedFeatureFalg.Id, JsonConvert.SerializeObject(updatedFeatureFalg));
        }


        [HttpGet]
        [Route("GetFeatureFlag")]
        public async Task<CosmosDBFeatureFlag> GetFeatureFlag(string id)
        {
            var currentUserId = this.HttpContext.User.Claims.FirstOrDefault(p => p.Type == "UserId").Value;
            CosmosDBFeatureFlag ff = await _cosmosDbService.GetCosmosDBFeatureFlagAsync(id);
           
            return ff;
        }

        [HttpPost]
        [Route("CreateFeatureFlag")]
        public async Task<CreateFeatureFlagViewModel> CreateFeatureFlag([FromBody] CreateFeatureFlagViewModel param)
        {
            var currentUserId = this.HttpContext.User.Claims.FirstOrDefault(p => p.Type == "UserId").Value;
            param.CreatorUserId = currentUserId;
            var ids = await _featureFlagService.GetAccountAndProjectIdByEnvironmentIdAsync(param.EnvironmentId);
            var newFF = await _cosmosDbService.CreateCosmosDBFeatureFlagAsync(param, currentUserId, ids[0], ids[1]);
            param.Id = newFF.Id;
            param.LastUpdatedTime = newFF.FF.LastUpdatedTime;
            param.KeyName = newFF.FF.KeyName;
            return param;
        }

        [HttpPut]
        [Route("UpdateFeatureFlag")]
        public async Task UpdateFeatureFlag([FromBody] CosmosDBFeatureFlag param)
        {
            var updatedFeatureFalg = await _cosmosDbService.UpdateCosmosDBFeatureFlagAsync(param);
            await _redisCache.SetStringAsync(updatedFeatureFalg.Id, JsonConvert.SerializeObject(updatedFeatureFalg));

            // 修改开关时，可以提示是否重新更新已有用户，如果是则异步的后台操作一次(这里有机会减少服务器负载量和云成本)，如果否则保持原有纪录不更新-只等待新用户-此时更新提示的timestamp保持不变)
        }

        [HttpPut]
        [Route("UpdateFeatureFlagSetting")]
        public async Task<CosmosDBFeatureFlagBasicInfo> UpdateFeatureFlagSetting([FromBody] CosmosDBFeatureFlagBasicInfo param)
        {
            var currentUserId = this.HttpContext.User.Claims.FirstOrDefault(p => p.Type == "UserId").Value;
            CosmosDBFeatureFlag ff = await _cosmosDbService.GetFeatureFlagAsync(param.Id);
            ff.FF.LastUpdatedTime = DateTime.UtcNow;
            ff.FF.Name = param.Name;
            await _cosmosDbService.UpdateCosmosDBFeatureFlagAsync(ff);
            param.LastUpdatedTime = ff.FF.LastUpdatedTime;
            param.KeyName = ff.FF.KeyName;
            return param;
        }

        [HttpDelete]
        [Route("DeleteFeatureFlag/{featureFlagId}")]
        public async Task DeleteFeatureFlag(string featureFlagId)
        {
            await _cosmosDbService.DeleteItemAsync(featureFlagId);
        }

        [HttpGet]
        [Route("GetEnvironmentUserProperties/{environmentId}")]
        public async Task<List<string>> GetEnvironmentUserProperties(int environmentId)
        {
            var p = await _cosmosDbService.GetCosmosDBEnvironmentUserPropertiesAsync(environmentId);
            if (p != null)
                return p.Properties ?? new List<string>();
            return new List<string>();
        }


    }
}
