using System.Collections.Generic;
using System.Threading.Tasks;
using FeatureFlags.APIs.Controllers.Base;
using FeatureFlags.APIs.Services;
using FeatureFlags.APIs.ViewModels;
using FeatureFlags.APIs.ViewModels.FeatureFlagsViewModels;
using Microsoft.AspNetCore.Mvc;

namespace FeatureFlags.APIs.Controllers
{
    [ApiVersion("2.0")]
    [Route("api/v{version:apiVersion}/envs/{envId:int}/feature-flag")]
    public class FeatureFlagV2Controller : ApiControllerBase
    {
        private readonly FeatureFlagV2Service _flagService;
        private readonly FeatureFlagV2AppService _flagAppService;

        public FeatureFlagV2Controller(
            FeatureFlagV2Service flagService,
            FeatureFlagV2AppService flagAppService)
        {
            _flagService = flagService;
            _flagAppService = flagAppService;
        }

        [HttpGet]
        public async Task<PagedResult<FeatureFlagListViewModel>> GetListAsync(
            int envId,
            [FromQuery] SearchFeatureFlagRequest request)
        {
            var pagedResult = await _flagAppService.GetListAsync(envId, request);

            return pagedResult;
        }

        [HttpGet]
        [Route("dropdown")]
        public async Task<List<DropdownItem>> GetDropDownsAsync(int envId)
        {
            var dropdowns = await _flagService.GetDropDownsAsync(envId);

            return dropdowns;
        }

        [HttpGet("is-name-used")]
        public async Task<bool>  IsNameUsedAsync(int envId, string name)
        {
            var isNameUsed = await _flagService.IsNameUsedAsync(envId, name);

            return isNameUsed;
        }
    }
}