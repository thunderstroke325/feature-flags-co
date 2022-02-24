using FeatureFlags.APIs.Models;

namespace FeatureFlags.APIs.ViewModels.Public
{
    public class FeatureFlagUserVariationRequest : FeatureFlagUser
    {
        public string FeatureFlagKeyName { get; set; }
    }
}