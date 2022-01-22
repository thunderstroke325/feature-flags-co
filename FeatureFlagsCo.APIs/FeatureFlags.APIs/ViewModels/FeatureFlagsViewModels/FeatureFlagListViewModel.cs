using System;

namespace FeatureFlags.APIs.ViewModels.FeatureFlagsViewModels
{
    public class FeatureFlagListViewModel
    {
        public string Id { get; set; }

        public string Name { get; set; }

        public string Tags { get; set; }

        public string Status { get; set; }

        public DateTime? LastModificationTime { get; set; }
    }
}