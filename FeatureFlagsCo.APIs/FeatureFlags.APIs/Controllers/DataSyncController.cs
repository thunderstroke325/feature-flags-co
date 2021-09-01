using System.IO;
using System.Linq;
using System.Threading.Tasks;
using FeatureFlags.APIs.Authentication;
using FeatureFlags.APIs.Repositories;
using FeatureFlags.APIs.Services;
using FeatureFlags.APIs.ViewModels.DataSync;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace FeatureFlags.APIs.Controllers
{
    //[Authorize(Roles = UserRoles.Admin)]
    [Authorize]
    [ApiController]
    [Route("api/datasync/envs/{envId}")]
    public class DataSyncController : ControllerBase
    {
        private readonly IGenericRepository _repository;
        private readonly ILogger<AccountsController> _logger;
        private readonly IEnvironmentService _envService;
        private readonly IDataSyncService _dataSyncService;

        public DataSyncController(ILogger<AccountsController> logger, IGenericRepository repository,
            IEnvironmentService envService,
            IDataSyncService dataSyncService)
        {
            _logger = logger;
            _repository = repository;
            _envService = envService;
            _dataSyncService = dataSyncService;
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

            return StatusCode(StatusCodes.Status403Forbidden, new Response { Status = "Error", Message = "Forbidden" });
        }

        [HttpPost]
        [Route("upload")]
        public async Task<dynamic> UploadEnvironmentData([FromForm]EnvironmentDataFileViewModel model, int envId)
        {
            var currentUserId = this.HttpContext.User.Claims.FirstOrDefault(p => p.Type == "UserId").Value;
            if (!await _envService.CheckIfUserHasRightToReadEnvAsync(currentUserId, envId))
            {
                return StatusCode(StatusCodes.Status403Forbidden, new Response { Status = "Error", Message = "Forbidden" });
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
    }
}
