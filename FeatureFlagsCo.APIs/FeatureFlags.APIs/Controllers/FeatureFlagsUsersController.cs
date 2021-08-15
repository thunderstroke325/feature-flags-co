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
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;

namespace FeatureFlags.APIs.Controllers
{
    //[Authorize(Roles = UserRoles.Admin)]
    [Authorize]
    [ApiController]
    [Route("[controller]")]
    public class FeatureFlagsUsersController : ControllerBase
    {
        private readonly IGenericRepository _repository;
        private readonly ILogger<FeatureFlagsUsersController> _logger;
        private readonly IDistributedCache _redisCache;
        private readonly IFeatureFlagsService _ffService;
        private readonly INoSqlService _nosqlDBService;
        private readonly IEnvironmentService _envService;

        public FeatureFlagsUsersController(
            ILogger<FeatureFlagsUsersController> logger, 
            IGenericRepository repository,
            IDistributedCache redisCache,
            IFeatureFlagsService ffService,
            INoSqlService cosmosDbService,
            IEnvironmentService envService)
        {
            _logger = logger;
            _repository = repository;
            _redisCache = redisCache;
            _ffService = ffService;
            _nosqlDBService = cosmosDbService;
            _envService = envService;
        }

        [HttpGet]
        [Route("QueryEnvironmentFeatureFlagUsers")]
        public async Task<dynamic> QueryEnvironmentFeatureFlagUsersAsync(string searchText, int environmentId, int pageIndex, int pageSize)
        {
            var currentUserId = this.HttpContext.User.Claims.FirstOrDefault(p => p.Type == "UserId").Value;
            if(await _envService.CheckIfUserHasRightToReadEnvAsync(currentUserId, environmentId))
            {
                return await _ffService.QueryEnvironmentFeatureFlagUsersAsync(searchText, environmentId, pageIndex, pageSize, currentUserId);
            }
            return null;
            
        }

        [HttpGet]
        [Route("GetFeatureFlagUser/{id}")]
        public async Task<EnvironmentFeatureFlagUser> GetFeatureFlagUser(string id)
        {
            return await _nosqlDBService.TrueFalseStatusGetEnvironmentFeatureFlagUserAsync(id);
        }

        [HttpGet]
        [Route("GetEnvironmentUser/{id}")]
        public async Task<EnvironmentUser> GetEnvironmentUser(string id)
        {
            return await _nosqlDBService.GetEnvironmentUserAsync(id);
        }
    }
}
