using FeatureFlags.APIs.Models;
using FeatureFlags.APIs.ViewModels.Metrics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FeatureFlags.APIs.ViewModels.Experiments
{
    public class ExperimentViewModel
    {
        public string Id { get; set; }
        public int EnvId { get; set; }
        public string FeatureFlagId { get; set; }
        public string FeatureFlagName { get; set; }
        public string MetricId { get; set; }
        public MetricViewModel Metric { get; set; }
        public string BaselineVariation { get; set; }
        public List<string> Variations { get; set; }
        public ExperimentStatus Status { get; set; }

        public List<ExperimentIteration> Iterations { get; set; }
    }

    public enum ExperimentStatus 
    {
        NotStarted = 1,
        NotRecording = 2,
        Recording = 3
    }
}
