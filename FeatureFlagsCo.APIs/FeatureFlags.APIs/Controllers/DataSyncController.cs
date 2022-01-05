using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using FeatureFlags.APIs.Authentication;
using FeatureFlags.APIs.Services;
using FeatureFlags.APIs.ViewModels.Analytic;
using FeatureFlags.APIs.ViewModels.DataSync;
using FeatureFlagsCo.MQ.ElasticSearch;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace FeatureFlags.APIs.Controllers
{
    //[Authorize(Roles = UserRoles.Admin)]
    [Authorize]
    [ApiController]
    [Route("api/datasync/envs/{envId}")]
    public class DataSyncController : ControllerBase
    {
        private readonly IEnvironmentService _envService;
        private readonly IDataSyncService _dataSyncService;
        private readonly ElasticSearchService _elasticSearch;

        public DataSyncController(
            IEnvironmentService envService,
            IDataSyncService dataSyncService, 
            ElasticSearchService elasticSearch)
        {
            _envService = envService;
            _dataSyncService = dataSyncService;
            _elasticSearch = elasticSearch;
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
            [FromQuery] SearchUserBehaviorRequest request)
        {
            request.Validate();

            var descriptor = request.SearchDescriptor();

            var response = await _elasticSearch.SearchDocumentAsync(descriptor);
            if (!response.IsValid)
            {
                throw new ElasticSearchException("search user behavior failed, please check to elastic search log.");
            }

            return response.Documents;
        }
    }
}
