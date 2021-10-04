using FeatureFlags.APIs.Authentication;
using FeatureFlags.APIs.Services;
using FeatureFlags.APIs.ViewModels.Experiments;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FeatureFlags.APIs.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class ExperimentsController : ControllerBase
    {
        private readonly ILogger<ExperimentsController> _logger;
        private readonly IEnvironmentService _envService;
        private readonly IExperimentsService _experimentsService;

        public ExperimentsController(
            ILogger<ExperimentsController> logger,
            IEnvironmentService envService,
            IExperimentsService experimentsService)
        {
            _logger = logger;
            _envService = envService;
            _experimentsService = experimentsService;
        }

        [HttpGet]
        [Route("Events/{envId}")]
        public async Task<dynamic> GetCustomEvents(int envId, [FromQuery]string lastItem, [FromQuery]string searchText)
        {
            var currentUserId = this.HttpContext.User.Claims.FirstOrDefault(p => p.Type == "UserId").Value;
            if (await _envService.CheckIfUserHasRightToReadEnvAsync(currentUserId, envId))
            {
               return await _experimentsService.GetEnvironmentEvents(envId, MetricTypeEnum.CustomEvent, lastItem, searchText);
            }

            return new List<string>();
        }

        [HttpPost]
        [Route("launchQuery/{envId}")]
        public async Task<List<ExperimentResultViewModel>> GetExperimentsResult(int envId, [FromBody]ExperimentQueryViewModel param)
        {
            var currentUserId = this.HttpContext.User.Claims.FirstOrDefault(p => p.Type == "UserId").Value;
            if (await _envService.CheckIfUserHasRightToReadEnvAsync(currentUserId, envId))
            {
                return await _experimentsService.GetExperimentResult(param);
            }

            return new List<ExperimentResultViewModel>();
        }

        [HttpPost]
        [Route("")]
        public async Task<dynamic> CreateExperiment([FromBody]ExperimentViewModel param)
        {
            var currentUserId = this.HttpContext.User.Claims.FirstOrDefault(p => p.Type == "UserId").Value;
            if (await _envService.CheckIfUserHasRightToReadEnvAsync(currentUserId, param.EnvId))
            {
                return await _experimentsService.CreateExperiment(param);
            }

            return StatusCode(StatusCodes.Status403Forbidden, new Response { Code = "Error", Message = "Forbidden" });
        }

        [HttpPut]
        [Route("{envId}/{experimentId}")]
        public async Task<dynamic> StartIteration(int envId, string experimentId)
        {
            var currentUserId = this.HttpContext.User.Claims.FirstOrDefault(p => p.Type == "UserId").Value;
            if (await _envService.CheckIfUserHasRightToReadEnvAsync(currentUserId, envId))
            {
                return await _experimentsService.StartIteration(envId, experimentId);
            }

            return StatusCode(StatusCodes.Status403Forbidden, new Response { Code = "Error", Message = "Forbidden" });
        }

        [HttpPut]
        [Route("{envId}/{experimentId}/{iterationId}")]
        public async Task<dynamic> StopIteration(int envId, string experimentId, string iterationId)
        {
            var currentUserId = this.HttpContext.User.Claims.FirstOrDefault(p => p.Type == "UserId").Value;
            if (await _envService.CheckIfUserHasRightToReadEnvAsync(currentUserId, envId))
            {
                return await _experimentsService.StopIteration(envId, experimentId, iterationId);
            }

            return StatusCode(StatusCodes.Status403Forbidden, new Response { Code = "Error", Message = "Forbidden" });
        }

        [HttpDelete]
        [Route("{envId}/{experimentId}")]
        public async Task ArchiveExperiment(int envId, string experimentId)
        {
            var currentUserId = this.HttpContext.User.Claims.FirstOrDefault(p => p.Type == "UserId").Value;
            if (await _envService.CheckIfUserHasRightToReadEnvAsync(currentUserId, envId))
            {
                await _experimentsService.ArchiveExperiment(experimentId);
            }

            return;
        }
    }
}
