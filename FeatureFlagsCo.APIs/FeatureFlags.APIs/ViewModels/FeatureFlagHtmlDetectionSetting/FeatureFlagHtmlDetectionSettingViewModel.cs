using FeatureFlags.APIs.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FeatureFlags.APIs.ViewModels.FeatureFlagHtmlDetectionSetting
{
    public class FeatureFlagHtmlDetectionSettingViewModel
    {
        public string FeatureFlagKey { get; set; }
        public List<CssSelectorItem> CssSelectors { get; set; }
    }
}
