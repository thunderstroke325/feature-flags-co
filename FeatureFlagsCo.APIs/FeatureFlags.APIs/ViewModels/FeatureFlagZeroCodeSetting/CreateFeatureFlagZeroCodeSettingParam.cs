using FeatureFlags.APIs.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FeatureFlags.APIs.ViewModels.FeatureFlagZeroCodeSetting
{
    public class CreateFeatureFlagZeroCodeSettingParam
    {
        public int EnvId { get; set; }
        public bool IsActive { get; set; }
        public string FeatureFlagId { get; set; }
        public string FeatureFlagKey { get; set; }
        public string EnvSecret { get; set; }
        public List<CssSelectorItem> Items { get; set; }
    }
}
