using FeatureFlags.APIs.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FeatureFlags.APIs.ViewModels.FeatureFlagZeroCodeSetting
{
    public class FeatureFlagZeroCodeSettingViewModel
    {
        public string FeatureFlagKey { get; set; }
        public List<CssSelectorItem> Items { get; set; }
    }
}
