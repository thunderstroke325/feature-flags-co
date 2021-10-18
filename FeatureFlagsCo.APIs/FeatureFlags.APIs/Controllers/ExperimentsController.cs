using FeatureFlags.APIs.Authentication;
using FeatureFlags.APIs.Models;
using FeatureFlags.APIs.Services;
using FeatureFlags.APIs.ViewModels.Experiments;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
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
        private readonly MongoDbFeatureFlagService _mongoDbFeatureFlagService;
        private readonly IEnvironmentService _environmentService;

        public ExperimentsController(
            ILogger<ExperimentsController> logger,
            IEnvironmentService envService,
            MongoDbFeatureFlagService mongoDbFeatureFlagService,
            IEnvironmentService environmentService,
            IExperimentsService experimentsService)
        {
            _logger = logger;
            _envService = envService;
            _experimentsService = experimentsService;
            _mongoDbFeatureFlagService = mongoDbFeatureFlagService;
            _environmentService = environmentService;
        }

        // SDK call this endpoint to get the active experiment settings
        [HttpGet]
        [AllowAnonymous]
        [Route("{envSecret}")]
        public async Task<IEnumerable<ExperimentMetricSetting>> GetActiveExperimentMetricSettings(string envSecret)
        {
            var envId = await _environmentService.GetEnvIdBySecretAsync(envSecret);
            return await _experimentsService.GetActiveExperimentMetricSettingsAsync(envId);
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

        [HttpGet]
        [Route("")]
        public async Task<dynamic> GetExperiments([FromQuery] int envId, [FromQuery] string searchText, [FromQuery] string featureFlagId = "", [FromQuery] int page = 0)
        {
            try
            {
                var currentUserId = this.HttpContext.User.Claims.FirstOrDefault(p => p.Type == "UserId").Value;
                if (await _envService.CheckIfUserHasRightToReadEnvAsync(currentUserId, envId))
                {
                    var featureFlagIds = new List<string>();
                    var featureFlags = new List<FeatureFlag>();

                    if (!string.IsNullOrWhiteSpace(featureFlagId))
                    {
                        var featureFlag = await _mongoDbFeatureFlagService.GetAsync(featureFlagId);
                        if (featureFlag != null) 
                        {
                            featureFlags.Add(featureFlag);
                            featureFlagIds.Add(featureFlagId);
                        }
                    } 
                    else 
                    {
                        featureFlags = await _mongoDbFeatureFlagService.SearchActiveAsync(envId, searchText, page, 50);
                        featureFlagIds = featureFlags.Select(f => f.Id).ToList();
                    }
                    
                    if (featureFlagIds.Any()) 
                    {
                        var experiments = await _experimentsService.GetExperimentsByFeatureFlagIds(featureFlagIds, !string.IsNullOrWhiteSpace(featureFlagId));
                        experiments.ForEach(ex => {
                            ex.FeatureFlagName = featureFlags.Find(f => f.Id == ex.FeatureFlagId)?.FF?.Name;
                        });

                        return experiments;
                    }

                    return new List<ExperimentViewModel>();
                }

                return StatusCode(StatusCodes.Status401Unauthorized, new Response { Code = "Error", Message = "Unauthorized" });
            }
            catch (Exception exp)
            {
                _logger.LogError(exp, JsonConvert.SerializeObject(new { EnvId = envId, searchText = searchText, Page = page }));

                return StatusCode(StatusCodes.Status500InternalServerError, new Response { Code = "Error", Message = "Internal Error" });
            }
        }

        [HttpPost]
        [Route("")]
        public async Task<dynamic> CreateExperiment([FromBody] ExperimentViewModel param)
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


        [HttpPost]
        [Route("{envId}")]
        public async Task<dynamic> GetIterationResults(int envId, [FromBody]List<ExperimentIterationTuple> experimentIterationTuples)
        {
            var currentUserId = this.HttpContext.User.Claims.FirstOrDefault(p => p.Type == "UserId").Value;
            if (await _envService.CheckIfUserHasRightToReadEnvAsync(currentUserId, envId))
            {
                return await _experimentsService.GetIterationResults(envId, experimentIterationTuples);
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

        [HttpDelete]
        [Route("{envId}/{experimentId}/data")]
        public async Task ArchiveExperimentData(int envId, string experimentId)
        {
            var currentUserId = this.HttpContext.User.Claims.FirstOrDefault(p => p.Type == "UserId").Value;
            if (await _envService.CheckIfUserHasRightToReadEnvAsync(currentUserId, envId))
            {
                await _experimentsService.ArchiveExperimentData(experimentId);
            }

            return;
        }
    }
}
