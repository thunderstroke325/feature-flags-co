using System;
using System.Collections.Generic;

namespace FeatureFlags.APIs.Models
{
    public class FeatureFlagUsageParam
    {
        public string UserName { get; set; }
        public string Email { get; set; }
        public string Country { get; set; }
        public string UserKeyId { get; set; }
        public List<FeatureFlagUserCustomizedProperty> UserCustomizedProperties { get; set; }
        public List<InsightUserVariation> UserVariations { get; set; }

        public bool IsValid()
        {
            var invalid =
                string.IsNullOrWhiteSpace(UserKeyId) ||
                UserVariations == null ||
                UserVariations.Count == 0;

            return !invalid;
        }
    }
}