using FeatureFlags.APIs.Authentication;
using FeatureFlags.APIs.Models;
using FeatureFlags.APIs.Services;
using FeatureFlags.APIs.ViewModels.FeatureFlagZeroCodeSetting;
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
    [Route("api/zero-code")]
    public class FeatureFlagZeroCodeSettingController : ControllerBase
    {
        private readonly ILogger<FeatureFlagZeroCodeSettingController> _logger;
        private readonly MongoDbFeatureFlagZeroCodeSettingService _mongoDbFFZCSService;
        private readonly IEnvironmentService _envService;
        private readonly MongoDbFeatureFlagService _mongoDbFeatureFlagService;

        public FeatureFlagZeroCodeSettingController(
            ILogger<FeatureFlagZeroCodeSettingController> logger,
            MongoDbFeatureFlagZeroCodeSettingService mongoDbFFZCSService,
            MongoDbFeatureFlagService mongoDbFeatureFlagService,
            IEnvironmentService envService)
        {
            _logger = logger;
            _mongoDbFFZCSService = mongoDbFFZCSService;
            _envService = envService;
            _mongoDbFeatureFlagService = mongoDbFeatureFlagService;
        }

        // TODO to remove
        [HttpGet]
        [AllowAnonymous]
        [Route("{envSecret}")]
        public async Task<List<FeatureFlagZeroCodeSettingViewModel>> GetFeatureFlagHtmlDetectionSettings(string envSecret)
        {
            var allSettings = await _mongoDbFFZCSService.GetByEnvSecretAsync(envSecret);
            if (allSettings != null && allSettings.Count > 0)
            {
                var featureFlags = await _mongoDbFeatureFlagService.GetActiveByIdsAsync(allSettings.Select(s => s.FeatureFlagId));
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


        [HttpPost]
        [Route("")]
        public async Task<dynamic> Upsert([FromBody]CreateFeatureFlagZeroCodeSettingParam param)
        {
            var currentUserId = this.HttpContext.User.Claims.FirstOrDefault(p => p.Type == "UserId").Value;
            if (await _envService.CheckIfUserHasRightToReadEnvAsync(currentUserId, param.EnvId))
            {
                var existedElements = await _mongoDbFFZCSService.CheckIfElementExistAlreadyAsync(param.FeatureFlagId);
                if(existedElements == null || existedElements.Count == 0)
                {
                    var ffHDSP = new FeatureFlagZeroCodeSetting
                    {
                        EnvId = param.EnvId,
                        EnvSecret = param.EnvSecret,
                        FeatureFlagId = param.FeatureFlagId,
                        FeatureFlagKey = param.FeatureFlagKey,
                        Items = param.Items,
                        IsActive = param.IsActive,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };
                    return await _mongoDbFFZCSService.CreateAsync(ffHDSP);
                }
                else
                {
                    var ffHDSP = new FeatureFlagZeroCodeSetting
                    {
                        Id = existedElements[0].Id,
                        EnvId = param.EnvId,
                        EnvSecret = param.EnvSecret,
                        FeatureFlagId = param.FeatureFlagId,
                        FeatureFlagKey = param.FeatureFlagKey,
                        Items = param.Items,
                        IsActive = param.IsActive,
                        UpdatedAt = DateTime.UtcNow
                    };

                    return await _mongoDbFFZCSService.UpdateAsync(ffHDSP.Id, ffHDSP);

                }
            }

            return StatusCode(StatusCodes.Status403Forbidden, new Response { Code = "Error", Message = "Forbidden" });
        }

        [HttpGet]
        [Route("{envId}/{ffId}")]
        public async Task<FeatureFlagZeroCodeSetting> Get(int envId, string ffId)
        {
            var currentUserId = this.HttpContext.User.Claims.FirstOrDefault(p => p.Type == "UserId").Value;
            if (await _envService.CheckIfUserHasRightToReadEnvAsync(currentUserId, envId))
            {
                return await _mongoDbFFZCSService.GetByEnvAndFeatureFlagIdAsync(envId, ffId);
            }
            Response.StatusCode = (int)HttpStatusCode.Unauthorized;
            return null;
        }
    }
}
