using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FeatureFlags.APIs.Authentication;
using FeatureFlags.APIs.Models;
using FeatureFlags.APIs.Repositories;
using FeatureFlags.APIs.Services;
using FeatureFlags.APIs.ViewModels.Environment;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace FeatureFlags.APIs.Controllers
{
    //[Authorize(Roles = UserRoles.Admin)]
    [Authorize]
    [ApiController]
    [Route("[controller]")]
    public class EnvironmentController : ControllerBase
    {
        private readonly IGenericRepository _repository;
        private readonly ILogger<EnvironmentController> _logger;
        private readonly IEnvironmentUserPropertyService _environmentService;
        private readonly IEnvironmentService _envService;

        public EnvironmentController(ILogger<EnvironmentController> logger, IGenericRepository repository,
            IEnvironmentUserPropertyService environmentService, IEnvironmentService envService)
        {
            _logger = logger;
            _repository = repository;
            _environmentService = environmentService;
            _envService = envService;
        }


        [HttpGet]
        [Route("GetEnvironmentUserProperties/{environmentId}")]
        public async Task<EnvironmentUserProperty> GetCosmosDBEnvironmentUserPropertiesForCRUDAsync(int environmentId)
        {
            var currentUserId = this.HttpContext.User.Claims.FirstOrDefault(p => p.Type == "UserId").Value;
            if (await _envService.CheckIfUserHasRightToReadEnvAsync(currentUserId, environmentId))
            {
                return await _environmentService.GetCosmosDBEnvironmentUserPropertiesForCRUDAsync(environmentId);
            }
            return null;
        }

        [HttpPost]
        [Route("CreateOrUpdateCosmosDBEnvironmentUserProperties")]
        public async Task CreateOrUpdateCosmosDBEnvironmentUserPropertiesForCRUDAsync([FromBody]EnvironmentUserProperty param)
        {
            var currentUserId = this.HttpContext.User.Claims.FirstOrDefault(p => p.Type == "UserId").Value;
            if (await _envService.CheckIfUserHasRightToReadEnvAsync(currentUserId, param.EnvironmentId))
            {
                await _environmentService.CreateOrUpdateCosmosDBEnvironmentUserPropertiesForCRUDAsync(param);
            }
        }
    }
}
