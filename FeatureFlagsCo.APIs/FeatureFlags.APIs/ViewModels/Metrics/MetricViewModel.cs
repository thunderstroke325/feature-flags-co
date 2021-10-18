using FeatureFlagsCo.MQ;
using System.Collections.Generic;

namespace FeatureFlags.APIs.ViewModels.Metrics
{
    public class MetricViewModel
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public int EnvId { get; set; }
        public string Description { get; set; }
        public string MaintainerUserId { get; set; }
        public string EventName { get; set; }
        public EventType EventType { get; set; }
        public CustomEventTrackOption CustomEventTrackOption { get; set; }
        public string CustomEventUnit { get; set; }
        public CustomEventSuccessCriteria CustomEventSuccessCriteria { get; set; }
        public string ElementTargets { get; set; }
        public List<TargetUrl> TargetUrls { get; set; }
    }
}
