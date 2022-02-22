using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using FeatureFlags.APIs.Controllers.Base;
using FeatureFlags.APIs.Models;
using FeatureFlags.APIs.Services;
using FeatureFlags.APIs.ViewModels.Project;
using Microsoft.AspNetCore.Mvc;

namespace FeatureFlags.APIs.Controllers
{
    [ApiVersion("2.0")]
    [Route("api/v{version:apiVersion}/envs/{envId:int}/settings")]
    public class EnvironmentSettingV2Controller : ApiControllerBase
    {
        private readonly EnvironmentSettingV2Service _service;

        public EnvironmentSettingV2Controller(EnvironmentSettingV2Service service)
        {
            _service = service;
        }
        
        [HttpGet]
        public async Task<IEnumerable<EnvironmentSettingV2>> GetSettingAsync(
            int envId, 
            [Required(AllowEmptyStrings = false)] string type)
        {
            var settings = await _service.GetAsync(envId, type);
            return settings;
        }

        [HttpPost]
        public async Task<IEnumerable<EnvironmentSettingV2>> UpsertSettingAsync(
            int envId, 
            IEnumerable<UpsertEnvironmentSetting> payload)
        {
            var settings = payload.Select(x => x.NewSetting());
            
            var updatedSettings = await _service.UpsertAsync(envId, settings);
            return updatedSettings;
        }
        
        [HttpDelete]
        public async Task<IEnumerable<EnvironmentSettingV2>> DeleteSettingAsync(
            int envId, 
            [Required(AllowEmptyStrings = false)] string settingId)
        {
            var updatedSettings = await _service.DeleteAsync(envId, settingId);
            return updatedSettings;
        }
    }
}