using FeatureFlagsCo.MQ;
using System;

namespace FeatureFlags.APIs.Models
{
    public class Metric: MongoModelBase
    {
        public string Name { get; set; }
        public int EnvId { get; set; }
        public string Description { get; set; }
        public string MaintainerUserId { get; set; }
        public string EventName { get; set; }
        public EventType EventType { get; set; }
        public CustomEventTrackOption CustomEventTrackOption { get; set; }
        public string CustomEventUnit { get; set; }
        public CustomEventSuccessCriteria CustomEventSuccessCriteria { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        // TODO add properties for page view and click

        public override string GetCollectionName()
        {
            return "Metrics";
        }
    }
}
