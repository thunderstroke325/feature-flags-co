using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FeatureFlags.APIs.Authentication;
using FeatureFlags.APIs.Models;
using FeatureFlags.APIs.Repositories;
using FeatureFlags.APIs.Services;
using FeatureFlags.APIs.ViewModels;
using FeatureFlags.APIs.ViewModels.Environment;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace FeatureFlags.APIs.Controllers
{
    //[Authorize(Roles = UserRoles.Admin)]
    //[Authorize]
    [ApiController]
    [Route("[controller]")]
    public class VariationController : ControllerBase
    {
        private readonly IGenericRepository _repository;
        private readonly ILogger<VariationController> _logger;
        private readonly IDistributedCache _redisCache;
        private readonly IVariationService _variationService;

        public VariationController(
            ILogger<VariationController> logger, 
            IGenericRepository repository,
            IDistributedCache redisCache,
            IVariationService variationService)
        {
            _logger = logger;
            _repository = repository;
            _redisCache = redisCache;
            _variationService = variationService;
        }


        [HttpPost]
        [Route("GetUserVariationResult")]
        public async Task<bool?> GetVariation([FromBody] GetUserVariationResultParam param)
        {
            Tuple<bool?, bool> returnResult = await GetVariationCore(param);

            return returnResult.Item1;
        }


        private async Task<Tuple<bool?, bool>> GetVariationCore(GetUserVariationResultParam param)
        {
            var ffIdVM = FeatureFlagKeyExtension.GetFeatureFlagIdByEnvironmentKey(param.EnvironmentSecret, param.FeatureFlagKeyName);
            var returnResult = await _variationService.CheckVariableAsync(param.EnvironmentSecret, param.FeatureFlagKeyName,
                new CosmosDBEnvironmentUser()
                {
                    Country = param.FFUserCountry,
                    CustomizedProperties = param.FFUserCustomizedProperties,
                    Email = param.FFUserEmail,
                    KeyId = param.FFUserKeyId,
                    Name = param.FFUserName
                },
                ffIdVM);
            var customizedTraceProperties = new Dictionary<string, object>()
            {
                ["envId"] = ffIdVM.EnvId,
                ["accountId"] = ffIdVM.AccountId,
                ["projectId"] = ffIdVM.ProjectId,
                ["featureFlagKey"] = param.FeatureFlagKeyName,
                ["userKey"] = param.FFUserKeyId,
                ["readOnlyOperation"] = returnResult.Item2,
            };
            using (_logger.BeginScope(customizedTraceProperties))
            {
                _logger.LogInformation("variation-request");
            }

            return returnResult;
        }

        [HttpPost]
        [Route("GetUserVariationResultInJson")]
        public async Task<GetUserVariationResultJsonViewModel> GetVariationInJson([FromBody] GetUserVariationResultParam param)
        {
            Tuple<bool?, bool> returnResult = await GetVariationCore(param);

            return new GetUserVariationResultJsonViewModel()
            {
                VariationResult = returnResult.Item1
            };
        }

        [HttpPost]
        [Route("GetMultiOptionVariation")]
        public async Task<ReturnJsonModel<VariationOption>> GetMultiOptionVariation([FromBody] GetUserVariationResultParam param)
        {
            try
            {
                var ffIdVM = FeatureFlagKeyExtension.GetFeatureFlagIdByEnvironmentKey(param.EnvironmentSecret, param.FeatureFlagKeyName);
                var returnResult = await _variationService.CheckMultiOptionVariationAsync(param.EnvironmentSecret, param.FeatureFlagKeyName,
                    new CosmosDBEnvironmentUser()
                    {
                        Country = param.FFUserCountry,
                        CustomizedProperties = param.FFUserCustomizedProperties,
                        Email = param.FFUserEmail,
                        KeyId = param.FFUserKeyId,
                        Name = param.FFUserName
                    },
                    ffIdVM);
                var customizedTraceProperties = new Dictionary<string, object>()
                {
                    ["envId"] = ffIdVM.EnvId,
                    ["accountId"] = ffIdVM.AccountId,
                    ["projectId"] = ffIdVM.ProjectId,
                    ["featureFlagKey"] = param.FeatureFlagKeyName,
                    ["userKey"] = param.FFUserKeyId,
                    ["readOnlyOperation"] = returnResult.Item2,
                };
                using (_logger.BeginScope(customizedTraceProperties))
                {
                    _logger.LogInformation("variation-request");
                }

                return new ReturnJsonModel<VariationOption>()
                {
                    StatusCode = 200,
                    Data = returnResult.Item1
                };
            }
            catch(Exception exp)
            {
                _logger.LogError(exp, "Post /Variation/GetMultiOptionVariation ; Body: " + JsonConvert.SerializeObject(param));
                return new ReturnJsonModel<VariationOption>()
                {
                    StatusCode = 500,
                    Error = exp
                };
            }
        }


        [HttpPost]
        [Route("VariationResultTest")]
        public async Task<bool?> GetVariationTest([FromBody] GetUserVariationResultParam param)
        {
            var ffIdVM = FeatureFlagKeyExtension.GetFeatureFlagIdByEnvironmentKey(param.EnvironmentSecret, param.FeatureFlagKeyName);
            var customizedTraceProperties = new Dictionary<string, object>()
            {
                ["envId"] = ffIdVM.EnvId,
                ["accountId"] = ffIdVM.AccountId,
                ["projectId"] = ffIdVM.ProjectId,
                ["featureFlagKey"] = param.FeatureFlagKeyName,
                ["userKey"] = param.FFUserKeyId
            };
            using (_logger.BeginScope(customizedTraceProperties))
            {
                _logger.LogInformation("variation-request");
            }

            return true;
        }

        [HttpGet]
        [Route("GetUserVariationResultPerformanceTest")]
        public async Task<bool?> GetVariationPerformance()
        {
            var key = "FakeUser-" + Guid.NewGuid().ToString();
            return await GetVariation(new GetUserVariationResultParam
            {
                FeatureFlagKeyName = "MQ==__cosmosdb-feature",
                FFUserCountry = "",
                FFUserCustomizedProperties = new List<FeatureFlagUserCustomizedProperty>() { 
                    new FeatureFlagUserCustomizedProperty
                    {
                        Name =  "TestProperty",
                        Value = key
                    }
                },
                FFUserEmail = key + "@user.co",
                FFUserKeyId = key,
                FFUserName = key,
                EnvironmentSecret = "ZGQ4NTkzM2ItNjRhNy00MDAwLTg2YTYtNTE4ZmY5YjgzYzUz"
            });
        }
    }
}
