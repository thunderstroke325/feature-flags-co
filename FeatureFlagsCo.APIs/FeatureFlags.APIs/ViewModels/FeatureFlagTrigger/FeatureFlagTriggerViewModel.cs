using System;

namespace FeatureFlags.APIs.ViewModels.FeatureFlagTrigger
{
    public class FeatureFlagTriggerViewModel
    {
        public string Id { get; set; }

        public FeatureFlagTriggerTypeEnum Type { get; set; }

        public FeatureFlagTriggerActionEnum Action { get; set; }

        public string Token { get; set; }

        public FeatureFlagTriggerStatusEnum Status { get; set; }

        public int Times { get; set; }

        public DateTime? UpdatedAt { get; set; }
        public DateTime? LastTriggeredAt { get; set; }

        public string Description { get; set; }

        public string FeatureFlagId { get; set; }
    }
}