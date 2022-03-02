using FeatureFlags.APIs.Services;
using System.Collections.Generic;

namespace FeatureFlags.APIs.Models
{
    public class InsightUser 
    {
        public string UserName { get; set; }
        public string Email { get; set; }
        public string Country { get; set; }
        public string KeyId { get; set; }
        public List<CustomizedProperty> CustomizedProperties { get; set; }
        
        public EnvironmentUser AsEnvironmentUser(int envId)
        {
            var envUser = new EnvironmentUser
            {
                Id = FeatureFlagKeyExtension.GetEnvironmentUserId(envId, KeyId),
                EnvironmentId = envId,
                KeyId = KeyId,
                Name = UserName,
                Email = Email,
                Country = Country,
                CustomizedProperties = CustomizedProperties,
            };

            return envUser;
        }
    }

    public class InsightParam
    {
        public InsightUser User { get; set; }
        public List<CustomizedProperty> CustomizedProperties { get; set; }
        public List<InsightUserVariationParam> UserVariations { get; set; }
        public List<InsightMetricParam> Metrics { get; set; }

        public bool IsValid()
        {
            var invalid =
                User == null ||
                string.IsNullOrWhiteSpace(User.KeyId) ||
                (
                    (UserVariations == null || UserVariations.Count == 0) && 
                    (Metrics == null || Metrics.Count == 0)
                );

            return !invalid;
        }
    }

    public class InsightUserVariationParam : UserVariation
    {
        public string FeatureFlagKeyName { get; set; }
        public override bool SendToExperiment { get; set; }
        public long Timestamp { get; set; }
        public InsightUserVariationParam(VariationOption variation) : base(variation)
        {
        }

        public InsightUserVariationParam()
        {
        }
    }

    public class InsightMetricParam
    {
        public string Route { get; set; }
        public string Type { get; set; } // pageview, click, customevent etc.
        public string EventName { get; set; }
        public float NumericValue { get; set; }
        public string AppType { get; set; } // javaserverside, javascript, ios, android etc.
    }
}
