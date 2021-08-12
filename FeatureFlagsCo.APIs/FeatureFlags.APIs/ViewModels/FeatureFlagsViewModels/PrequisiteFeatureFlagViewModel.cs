using FeatureFlags.APIs.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FeatureFlags.APIs.ViewModels.FeatureFlagsViewModels
{
    public class PrequisiteFeatureFlagViewModel
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string KeyName { get; set; }
        public int EnvironmentId { get; set; }
        public List<VariationOption> VariationOptions { get; set; }
    }
}
