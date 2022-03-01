using System.Linq;
using System.Threading.Tasks;
using FeatureFlags.APIs.Models;
using FeatureFlags.APIs.Repositories;
using FeatureFlags.APIs.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FeatureFlags.APIs.Controllers
{
    [Authorize]
    [ApiController]
    [Route("[controller]")]
    public class FeatureFlagsUsersController : ControllerBase
    {
        private readonly IFeatureFlagsService _ffService;
        private readonly INoSqlService _nosqlDBService;
        private readonly IEnvironmentService _envService;

        public FeatureFlagsUsersController(
            IFeatureFlagsService ffService,
            INoSqlService cosmosDbService,
            IEnvironmentService envService)
        {
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
        [Route("GetEnvironmentUser/{id}")]
        public async Task<EnvironmentUser> GetEnvironmentUser(string id)
        {
            return await _nosqlDBService.GetEnvironmentUserAsync(id);
        }
    }
}
