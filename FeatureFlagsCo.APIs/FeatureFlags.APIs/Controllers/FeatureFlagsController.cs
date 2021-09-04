using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FeatureFlags.APIs.Models;
using FeatureFlags.APIs.Repositories;
using FeatureFlags.APIs.Services;
using FeatureFlags.APIs.ViewModels;
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
        private readonly INoSqlService _noSqlDbService;
        private readonly IDistributedCache _redisCache;
        private readonly IEnvironmentService _envService;

        public FeatureFlagsController(ILogger<FeatureFlagsController> logger, IGenericRepository repository,
            IFeatureFlagsService featureFlagService,
            INoSqlService noSqlDbService,
            IDistributedCache redisCache,
            IEnvironmentService envService)
        {
            _logger = logger;
            _repository = repository;
            _featureFlagService = featureFlagService;
            _noSqlDbService = noSqlDbService;
            _redisCache = redisCache;

            _envService = envService;
        }


        [HttpGet]
        [Route("GetEnvironmentFeatureFlags/{environmentId}")]
        public async Task<List<FeatureFlagBasicInfo>> GetEnvironmentFeatureFlags(int environmentId)
        {
            // TODO pagination
            return await _noSqlDbService.GetEnvironmentFeatureFlagBasicInfoItemsAsync(environmentId, 0, 300);
        }

        [HttpGet]
        [Route("SearchPrequisiteFeatureFlags")]
        public async Task<List<PrequisiteFeatureFlagViewModel>> SearchPrequisiteFeatureFlags(int environmentId, string searchText = "", int pageIndex = 0, int pageSize = 20)
        {
            var currentUserId = this.HttpContext.User.Claims.FirstOrDefault(p => p.Type == "UserId").Value;
            if (await _envService.CheckIfUserHasRightToReadEnvAsync(currentUserId, environmentId))
            {
                pageSize = Math.Min(pageSize < 1 ? 20 : pageSize, 20);
                return await _noSqlDbService.SearchPrequisiteFeatureFlagsAsync(environmentId, searchText, pageIndex, pageSize);
            }

            return new List<PrequisiteFeatureFlagViewModel>();
        }


        [HttpGet]
        [Route("GetEnvironmentArchivedFeatureFlags/{environmentId}")]
        public async Task<List<FeatureFlagBasicInfo>> GetEnvironmentArchivedFeatureFlags(int environmentId)
        {
            return await _noSqlDbService.GetEnvironmentArchivedFeatureFlagBasicInfoItemsAsync(environmentId, 0, 300);
        }

        [HttpPost]
        [Route("ArchiveEnvironmentdFeatureFlag")]
        public async Task<FeatureFlag> ArchiveEnvironmentdFeatureFlag([FromBody] FeatureFlagArchiveParam param)
        {
            await _redisCache.RemoveAsync(param.FeatureFlagId);
            return await _noSqlDbService.ArchiveEnvironmentdFeatureFlagAsync(param);
        }

        [HttpPost]
        [Route("UnarchiveEnvironmentdFeatureFlag")]
        public async Task<FeatureFlag> UnarchiveEnvironmentdFeatureFlag([FromBody] FeatureFlagArchiveParam param)
        {
            await _redisCache.RemoveAsync(param.FeatureFlagId);
            return await _noSqlDbService.UnarchiveEnvironmentdFeatureFlagAsync(param);
        }

        [HttpPost]
        [Route("SwitchFeatureFlag")]
        public async Task SwitchFeatureFlag([FromBody] FeatureFlagBasicInfo param)
        {
            FeatureFlag ff = await _noSqlDbService.GetFeatureFlagAsync(param.Id);
            ff.FF.LastUpdatedTime = DateTime.UtcNow;
            ff.FF.Status = param.Status;
            var updatedFeatureFalg = await _noSqlDbService.UpdateFeatureFlagAsync(ff);
            await _redisCache.SetStringAsync(updatedFeatureFalg.Id, JsonConvert.SerializeObject(updatedFeatureFalg));
        }


        [HttpGet]
        [Route("GetFeatureFlag")]
        public async Task<FeatureFlag> GetFeatureFlag(string id)
        {
            var currentUserId = this.HttpContext.User.Claims.FirstOrDefault(p => p.Type == "UserId").Value;
            FeatureFlag ff = await _noSqlDbService.GetFlagAsync(id);

            return ff;
        }

        // The multi state FF is supported
        [HttpPost]
        [Route("CreateFeatureFlag")]
        public async Task<CreateFeatureFlagViewModel> CreateFeatureFlag([FromBody] CreateFeatureFlagViewModel param)
        {
            var currentUserId = this.HttpContext.User.Claims.FirstOrDefault(p => p.Type == "UserId").Value;
            param.CreatorUserId = currentUserId;
            var ids = await _featureFlagService.GetAccountAndProjectIdByEnvironmentIdAsync(param.EnvironmentId);
            var newFF = await _noSqlDbService.CreateFeatureFlagAsync(param, currentUserId, ids[0], ids[1]);
            param.Id = newFF.Id;
            param.KeyName = newFF.FF.KeyName;
            return param;
        }

        [HttpPut]
        [Route("UpdateFeatureFlag")]
        public async Task UpdateFeatureFlag([FromBody] FeatureFlag param)
        {
            var updatedFeatureFalg = await _noSqlDbService.UpdateFeatureFlagAsync(param);
            await _redisCache.SetStringAsync(updatedFeatureFalg.Id, JsonConvert.SerializeObject(updatedFeatureFalg));

            // 修改开关时，可以提示是否重新更新已有用户，如果是则异步的后台操作一次(这里有机会减少服务器负载量和云成本)，如果否则保持原有纪录不更新-只等待新用户-此时更新提示的timestamp保持不变)
        }

        // The multi state FF is supported
        [HttpPut]
        [Route("UpdateFeatureFlagSetting")]
        public async Task<FeatureFlagSettings> UpdateFeatureFlagSetting([FromBody] FeatureFlagSettings param)
        {
            var currentUserId = this.HttpContext.User.Claims.FirstOrDefault(p => p.Type == "UserId").Value;
            FeatureFlag ff = await _noSqlDbService.GetFeatureFlagAsync(param.Id);
            ff.FF.LastUpdatedTime = DateTime.UtcNow;
            ff.FF.Name = param.Name;

            ff.VariationOptions = param.VariationOptions;
            ff.FF.VariationOptionWhenDisabled = param.VariationOptions.FirstOrDefault(o => o.LocalId == ff.FF.VariationOptionWhenDisabled.LocalId);
            ff.FFTUWMTR.ForEach(f => {
                f.ValueOptionsVariationRuleValues.ForEach(v => v.ValueOption = param.VariationOptions.FirstOrDefault(o => o.LocalId == v.ValueOption.LocalId));
            });
            ff.TargetIndividuals.ForEach(t => t.ValueOption = param.VariationOptions.FirstOrDefault(v => v.LocalId == t.ValueOption.LocalId));

            // update prerequistes
            foreach (var f in ff.FFP)
            {
                var prerequisite = await _noSqlDbService.GetFeatureFlagAsync(f.PrerequisiteFeatureFlagId);
                f.ValueOptionsVariationValue = prerequisite.VariationOptions.FirstOrDefault(v => v.LocalId == f.ValueOptionsVariationValue.LocalId);
            }
            
            await _noSqlDbService.UpdateFeatureFlagAsync(ff);
            param.LastUpdatedTime = ff.FF.LastUpdatedTime;
            param.KeyName = ff.FF.KeyName;
            return param;
        }

        [HttpGet]
        [Route("GetEnvironmentUserProperties/{environmentId}")]
        public async Task<List<string>> GetEnvironmentUserProperties(int environmentId)
        {
            var currentUserId = this.HttpContext.User.Claims.FirstOrDefault(p => p.Type == "UserId").Value;
            if (await _envService.CheckIfUserHasRightToReadEnvAsync(currentUserId, environmentId))
            {
                var p = await _noSqlDbService.GetEnvironmentUserPropertiesAsync(environmentId);
                if (p != null)
                    return p.Properties ?? new List<string>();
            }

            return new List<string>();
        }

        #region multi variation options

        [HttpPut]
        [Route("UpdateMultiOptionSupportedFeatureFlag")]
        public async Task<ReturnJsonModel<FeatureFlag>> UpdateMultiOptionSupportedFeatureFlag([FromBody] FeatureFlag param)
        {
            var currentUserId = this.HttpContext.User.Claims.FirstOrDefault(p => p.Type == "UserId").Value;
            if (await _envService.CheckIfUserHasRightToReadEnvAsync(currentUserId, param.EnvironmentId))
            {
                if (await _noSqlDbService.GetFlagAsync(param.Id) == null)
                {
                    return new ReturnJsonModel<FeatureFlag>()
                    {
                        StatusCode = 404,
                        Error = new Exception("Not Found")
                    };
                }

                var returnOBj = await _noSqlDbService.UpdateMultiValueOptionSupportedFeatureFlagAsync(param);
                if (returnOBj.StatusCode == 200)
                {
                    await _redisCache.SetStringAsync(returnOBj.Data.Id, JsonConvert.SerializeObject(returnOBj.Data));
                }
                else
                {
                    _logger.LogError(returnOBj.Error, JsonConvert.SerializeObject(param));
                }
                return returnOBj;
            }
            return new ReturnJsonModel<FeatureFlag>()
            {
                StatusCode = 401,
                Error = new Exception("Unauthorized")
            };
        }

        #endregion
    }
}
