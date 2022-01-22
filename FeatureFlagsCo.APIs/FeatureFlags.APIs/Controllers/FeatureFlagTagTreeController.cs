using System.Collections.Generic;
using System.Threading.Tasks;
using FeatureFlags.APIs.Controllers.Base;
using FeatureFlags.APIs.Models;
using FeatureFlags.APIs.Services;
using Microsoft.AspNetCore.Mvc;

namespace FeatureFlags.APIs.Controllers
{
    [Route("api/v{version:apiVersion}/envs/{envId:int}/feature-flag-tag-tree")]
    public class FeatureFlagTagTreeController : ApiControllerBase
    {
        private readonly FeatureFlagTagTreeService _service;

        public FeatureFlagTagTreeController(FeatureFlagTagTreeService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<FeatureFlagTagTrees> GetAsync(int envId)
        {
            var tagTrees = await _service.GetAsync(envId);

            return tagTrees;
        }

        [HttpPost]
        public async Task<FeatureFlagTagTrees> SaveAsync(int envId, List<FeatureFlagTagTree> trees)
        {
            var tagTrees = await _service.UpsertAsync(envId, trees);

            return tagTrees;
        }
    }
}