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
        public FeatureFlagType FeatureFlagType { get; set; }
        public List<CssSelectorItemViewModel> Items { get; set; }
    }

    public class CssSelectorItemViewModel 
    {
        public string CssSelector { get; set; }
        public string VariationValue { get; set; }
        public int VariationOptionId { get; set; }
        public string Url { get; set; }
    }
}
