using FeatureFlagsCo.MQ;
using System.Collections.Generic;

namespace FeatureFlags.APIs.ViewModels.Experiments
{
    public class ExperimentMetricSetting
    {
        public string EventName { get; set; }
        public EventType EventType { get; set; }
        public string ElementTargets { get; set; }
        public List<TargetUrl> TargetUrls { get; set; }
    }
}
