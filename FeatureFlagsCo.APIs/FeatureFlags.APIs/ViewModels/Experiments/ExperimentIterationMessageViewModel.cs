using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FeatureFlags.APIs.ViewModels.Experiments
{
    public class ExperimentIterationMessageViewModel
    {
        public string ExptId { get; set; }
        public string EnvId { get; set; }
        public string EventName { get; set; }
        public string StartExptTime { get; set; }
        public string EndExptTime { get; set; }
        public string IterationId { get; set; }
        public ExperimentFeatureFlagViewModel Flag { get; set; }
    }
}
