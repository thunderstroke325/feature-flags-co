using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using FeatureFlags.APIs.Authentication;
using FeatureFlags.APIs.Models;
using FeatureFlags.APIs.Services;
using FeatureFlags.APIs.ViewModels.Analytic;
using FeatureFlags.APIs.ViewModels.DataSync;
using FeatureFlags.Utils.ExtensionMethods;
using FeatureFlagsCo.MQ.ElasticSearch;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace FeatureFlags.APIs.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/datasync/envs/{envId:int}")]
    public class DataSyncController : ControllerBase
    {
        private readonly IEnvironmentService _envService;
        private readonly DataSyncService _dataSyncService;
        private readonly IMapper _mapper;

        public DataSyncController(
            IEnvironmentService envService,
            DataSyncService dataSyncService, 
            IMapper mapper)
        {
            _envService = envService;
            _dataSyncService = dataSyncService;
            _mapper = mapper;
        }

        [HttpGet]
        [Route("download")]
        public async Task<dynamic> GetEnvironmentData(int envId)
        {
            var currentUserId = this.HttpContext.User.Claims.FirstOrDefault(p => p.Type == "UserId").Value;
            if (await _envService.CheckIfUserHasRightToReadEnvAsync(currentUserId, envId))
            {
                return await _dataSyncService.GetEnvironmentDataAsync(envId);
            }

            return StatusCode(StatusCodes.Status403Forbidden, new Response { Code = "Error", Message = "Forbidden" });
        }

        [HttpPost]
        [Route("upload")]
        public async Task<dynamic> UploadEnvironmentData([FromForm]EnvironmentDataFileViewModel model, int envId)
        {
            var currentUserId = this.HttpContext.User.Claims.FirstOrDefault(p => p.Type == "UserId").Value;
            if (!await _envService.CheckIfUserHasRightToReadEnvAsync(currentUserId, envId))
            {
                return StatusCode(StatusCodes.Status403Forbidden, new Response { Code = "Error", Message = "Forbidden" });
            }

            if (model.File.Length > 0)
            {
                var content = string.Empty;
                using (var reader = new StreamReader(model.File.OpenReadStream()))
                {
                    content = await reader.ReadToEndAsync();
                }

                EnvironmentDataViewModel data = null;
                try
                {
                    data = JsonConvert.DeserializeObject<EnvironmentDataViewModel>(content);
                    await _dataSyncService.SaveEnvironmentDataAsync(envId, data);
                }
                catch
                {
                    return BadRequest();
                }
            }

            return Ok();
        }

        [HttpGet]
        [Route("user-behavior")]
        public async Task<IEnumerable<TrackUserBehaviorEvent>> SearchUserBehaviorAsync(
            [FromQuery] SearchUserBehaviorRequest request,
            [FromServices] ElasticSearchService elasticSearch)
        {
            request.Validate();

            var descriptor = request.SearchDescriptor();

            var response = await elasticSearch.SearchDocumentAsync(descriptor);
            if (!response.IsValid)
            {
                throw new ElasticSearchException("search user behavior failed, please check to elastic search log.");
            }

            return response.Documents;
        }

        [HttpPut("sync-to-remote")]
        public async Task<string> SyncToRemoteAsync(
            int envId, 
            [Required(AllowEmptyStrings = false)] string settingId, 
            [FromServices] EnvironmentV2Service envService, 
            [FromServices] FeatureFlagV2Service flagService)
        {
            // get env
            var env = await envService.GetAsync(envId);
            var setting =
                env.Settings.FirstOrDefault(x => x.Type == EnvironmentSettingTypes.SyncUrls && x.Id == settingId);
            if (setting == null)
            {
                return "false,0";
            }
            
            // sync data to remote
            // same format with 'data-sync' streaming for server-side sdk
            var flags = await flagService.GetActiveFlagsAsync(envId);
            
            var sdkFlags = _mapper.Map<IEnumerable<FeatureFlag>, IEnumerable<ServerSdkFeatureFlag>>(flags);
            var dataToSync = new 
            {
                eventType = SdkDataSyncTypes.Full, 
                featureFlags = sdkFlags.OrderByDescending(flag => flag.Timestamp)
            };

            var message = new SdkWebSocketMessage(SdkWebSocketMessageTypes.DataSync, dataToSync);

            var syncSuccess = await _dataSyncService.SyncToRemoteAsync(env, setting.Value, message);
            var timestamp = DateTime.UtcNow.UnixTimestampInMilliseconds();
            var remark = $"{syncSuccess.ToString().ToLowerInvariant()},{timestamp}";
            
            // write remark to setting & save
            setting.WriteRemark(remark);
            await envService.UpdateAsync(env);

            return remark;
        }
    }
}
