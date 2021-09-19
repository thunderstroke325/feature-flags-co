using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FeatureFlags.APIs.ViewModels.Experiments
{
    public class ExperimentQueryViewModel
    {
        public string EventName { get; set; }
        public string StartExptTime { get; set; }
        public string EndExptTime { get; set; }
        public ExperimentFeatureFlagViewModel Flag { get; set; }
    }

    public class ExperimentFeatureFlagViewModel
    {
        public string Id { get; set; }
        public string BaselineVariation { get; set; }
        public List<string> Variations { get; set; }
    }
}
