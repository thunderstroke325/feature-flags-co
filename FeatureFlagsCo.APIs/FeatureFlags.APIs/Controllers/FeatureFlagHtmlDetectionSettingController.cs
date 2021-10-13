using FeatureFlags.APIs.Authentication;
using FeatureFlags.APIs.Models;
using FeatureFlags.APIs.Services;
using FeatureFlags.APIs.ViewModels.Experiments;
using FeatureFlags.APIs.ViewModels.FeatureFlagHtmlDetectionSetting;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace FeatureFlags.APIs.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class FeatureFlagHtmlDetectionSettingController : ControllerBase
    {
        private readonly ILogger<FeatureFlagHtmlDetectionSettingController> _logger;
        private readonly MongoDbFeatureFlagHtmlDetectionSettingService _mongoDbFFHDSService;
        private readonly IEnvironmentService _envService;

        public FeatureFlagHtmlDetectionSettingController(
            ILogger<FeatureFlagHtmlDetectionSettingController> logger,
            MongoDbFeatureFlagHtmlDetectionSettingService mongoDbFFHDSService,
            IEnvironmentService envService)
        {
            _logger = logger;
            _mongoDbFFHDSService = mongoDbFFHDSService;
            _envService = envService;
        }

        [HttpGet]
        [AllowAnonymous]
        [Route("Settings/{environmentKey}")]
        public async Task<List<FeatureFlagHtmlDetectionSettingViewModel>> GetFeatureFlagHtmlDetectionSettings(string environmentKey)
        {
            var allSettings = await _mongoDbFFHDSService.GetFeatureFlagHtmlDetectionSettingsAsync(environmentKey);
            if (allSettings != null && allSettings.Count > 0)
                return allSettings.Select(p => new FeatureFlagHtmlDetectionSettingViewModel()
                {
                    CssSelectors = p.Items,
                    FeatureFlagKey = p.FeatureFlagKey
                }).ToList();
            return new List<FeatureFlagHtmlDetectionSettingViewModel>();
        }


        [HttpPost]
        [Route("")]
        public async Task<FeatureFlagHtmlDetectionSetting> Create([FromBody]CreateFeatureFlagHtmlDetectionSettingParam param)
        {
            var currentUserId = this.HttpContext.User.Claims.FirstOrDefault(p => p.Type == "UserId").Value;
            if (await _envService.CheckIfUserHasRightToReadEnvAsync(currentUserId, param.EnvironmentId))
            {
                var existedElements = await _mongoDbFFHDSService.CheckIfElementExistAlreadyAsync(param.FeatureFlagId);
                if(existedElements == null || existedElements.Count == 0)
                {
                    var ffHDSP = new FeatureFlagHtmlDetectionSetting
                    {
                        EnvironmentId = param.EnvironmentId,
                        EnvironmentKey = param.EnvironmentKey,
                        FeatureFlagId = param.FeatureFlagId,
                        FeatureFlagKey = param.FeatureFlagKey,
                        Items = param.Items,
                        IsActive = param.IsActive,
                        UpdatedDate = DateTime.UtcNow
                    };
                    return await _mongoDbFFHDSService.CreateAsync(ffHDSP);
                }
                else
                {
                    return await Update(new UpdateFeatureFlagHtmlDetectionSettingParam
                    {
                        EnvironmentId = param.EnvironmentId,
                        EnvironmentKey = param.EnvironmentKey,
                        FeatureFlagId = param.FeatureFlagId,
                        Id = existedElements[0].Id,
                        Items = param.Items,
                        IsActive = param.IsActive
                    });
                }
            }
            Response.StatusCode = (int)HttpStatusCode.Unauthorized;
            return null;
        }

        [HttpPut]
        [Route("")]
        public async Task<FeatureFlagHtmlDetectionSetting> Update([FromBody] UpdateFeatureFlagHtmlDetectionSettingParam param)
        {
            var currentUserId = this.HttpContext.User.Claims.FirstOrDefault(p => p.Type == "UserId").Value;
            if (await _envService.CheckIfUserHasRightToReadEnvAsync(currentUserId, param.EnvironmentId))
            {
                var ffHDSP = new FeatureFlagHtmlDetectionSetting
                {
                    Id = param.Id,
                    EnvironmentId = param.EnvironmentId,
                    EnvironmentKey = param.EnvironmentKey,
                    FeatureFlagId = param.FeatureFlagId,
                    FeatureFlagKey = param.FeatureFlagKey,
                    Items = param.Items,
                    IsActive = param.IsActive,
                    UpdatedDate = DateTime.UtcNow
                };
                await _mongoDbFFHDSService.UpdateAsync(ffHDSP.Id, ffHDSP);
                return await Get(param.EnvironmentId, param.Id);
            }
            Response.StatusCode = (int)HttpStatusCode.Unauthorized;
            return null;
        }

        [HttpGet]
        [Route("{envId}/{id}")]
        public async Task<FeatureFlagHtmlDetectionSetting> Get(int envId, string id)
        {
            var currentUserId = this.HttpContext.User.Claims.FirstOrDefault(p => p.Type == "UserId").Value;
            if (await _envService.CheckIfUserHasRightToReadEnvAsync(currentUserId, envId))
            {
                return await _mongoDbFFHDSService.GetAsync(id);
            }
            Response.StatusCode = (int)HttpStatusCode.Unauthorized;
            return null;
        }
    }
}
